using FastLane.Chatbot.Contract.Actions;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Contract.Services;

/// <summary>
/// WhatsApp browser routines
/// </summary>
public interface IWhatsAppClient : IAsyncDisposable
{
	/// <summary>
	/// New message was received
	/// </summary>
	event Action? MessageReceived;

	/// <summary>
	/// Read last available message from chat with given name(user name)
	/// </summary>
	Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string chat, CancellationToken cancellationToken);

	/// <summary>
	/// Get list of chats with number of unread messages inside. (Calls <see cref="MessageReceived"/> event if statistics was changed since last query).
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IReadOnlyDictionary<string, int>> GetChatInboxCountAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Open chat with specified name, send message and close chat
	/// </summary>
	Task PostAsync(string chat, string content, CancellationToken cancellationToken);

}

internal class WhatsAppClient : IWhatsAppClient, IDisposable
{
	private readonly IBrowser _browser;
	private readonly ILogger<WhatsAppClient> _logger;
	private readonly IOptionsMonitor<Settings> _settings;
	private IReadOnlyDictionary<string, int> _currentChatUnreadMessages = new Dictionary<string, int>();
	private readonly WhatsAppClientsPool _whatsAppClientsPool;
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public event Action? MessageReceived;

	public WhatsAppClient(
		IBrowser browser,
		ILogger<WhatsAppClient> logger,
		IOptionsMonitor<Settings> settings,
		WhatsAppClientsPool whatsAppClientsPool)
	{
		_browser = browser;
		_logger = logger;
		_settings = settings;
		_whatsAppClientsPool = whatsAppClientsPool;
	}

	public async ValueTask DisposeAsync()
	{
		await _browser.DisposeAsync();
	}

	public async Task WaitForLoginAsync(CancellationToken cancellationToken)
	{
		try
		{
			IPage page = await GetPageAsync();
			await page.GoToAsync("https://web.whatsapp.com", WaitUntilNavigation.Networkidle2);

			_logger.LogInformation("Wait for login");
			while (!cancellationToken.IsCancellationRequested && !await IsLoggedInAsync(cancellationToken))
			{
				await Task.Delay(1000, cancellationToken);
			}

			cancellationToken.ThrowIfCancellationRequested();
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Cancelled");
			throw;
		}
	}

	private async Task<bool> IsLoggedInAsync(CancellationToken cancellationToken)
	{
		IPage page = await GetPageAsync();
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				const string SearchInputXpath = "xpath///div[@contenteditable and @role='textbox']";
				if (await page.WaitForSelectorAsync(SearchInputXpath) is not null)
				{
					return true;
				}
			}
			catch
			{
				try
				{
					await Task.Delay(1000, cancellationToken);
				}
				catch
				{
					return false;
				}
			}
		}

		return false;
	}

	private async Task<IPage> GetPageAsync()
	{
		IPage[] pages = await _browser.PagesAsync();
		return pages.FirstOrDefault() ?? await _browser.NewPageAsync();
	}

	public async Task<IReadOnlyDictionary<string, int>> GetChatInboxCountAsync(CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);
		try
		{
			IReadOnlyDictionary<string, int> newStat = await new GetUnreadMessagesStatistics(_settings).InvokeActionAsync(_browser, cancellationToken);

			if (MessageReceived != null &&
				!((newStat == _currentChatUnreadMessages) || (newStat.Count == _currentChatUnreadMessages.Count && !newStat.Except(_currentChatUnreadMessages).Any()))
				)
			{
				MessageReceived?.Invoke();
			}

			return _currentChatUnreadMessages = newStat;
		}
		finally { _semaphore.Release(); }
	}

	/// <summary>
	/// Filter chat list by typing text into Search box
	/// </summary>
	/// <param name="searchText">Chat name piece to enter into search box</param>
	public async Task<object> FilterChatsAsync(string searchText, CancellationToken cancellationToken)
	{
		return await new InputSearchBox(_settings).InvokeActionAsync((_browser, searchText), cancellationToken);
	}

	/// <summary>
	/// Enters to specified chat
	/// </summary>
	/// <param name="chatName">Name of chat to open</param>
	/// <returns>True if operation was successfull</returns>
	public async Task<bool> EnterChatAsync(string chatName, CancellationToken cancellationToken)
	{
		return await new EnterChat(_settings).InvokeActionAsync((_browser, chatName), cancellationToken);
	}

	/// <summary>
	/// Closes currently active chat
	/// </summary>
	/// <param name="cancellationToken"></param>
	public async Task<bool> CloseChatAsync(CancellationToken cancellationToken)
	{
		return await new CloseChat(_settings).InvokeActionAsync(_browser, cancellationToken);
	}

	/// <summary>
	/// Read last available message from chat with given name(user name)
	/// </summary>
	public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string chat, CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);
		try
		{
			if (!await EnterChatAsync(chat, cancellationToken))
			{ throw new InvalidOperationException("Unabled to enter chat " + chat); }

			IReadOnlyList<ChatMessage> result = await new GetLastMessages(_settings).InvokeActionAsync(_browser, ChatMember.Bot | ChatMember.User, cancellationToken);

			return !await CloseChatAsync(cancellationToken) ? throw new InvalidOperationException("Unabled to close active chat ") : result;
		}
		finally
		{ _semaphore.Release(); }
	}

	public async Task PostAsync(string chat, string content, CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);
		try
		{
			await EnterChatAsync(chat, cancellationToken);
			await new PostMessage(_settings).InvokeActionAsync((_browser, content), cancellationToken);
			await CloseChatAsync(cancellationToken);
		}
		finally { _semaphore.Release(); }
	}

	public void Dispose()
	{
		if (_whatsAppClientsPool != null && _whatsAppClientsPool.Count != 0)
		{
			if (_whatsAppClientsPool.Count == 1)
			{ _whatsAppClientsPool?.TryTake(out IWhatsAppClient? _); }
			else
			{
				IWhatsAppClient[] backClients = _whatsAppClientsPool.ToArray();
				_whatsAppClientsPool.Clear();

				foreach (IWhatsAppClient client in backClients)
				{
					if (client != this)
					{ _whatsAppClientsPool.Add(client); }
				}
			}
		}
	}
}