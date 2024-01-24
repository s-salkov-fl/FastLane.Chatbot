using System.Collections.Concurrent;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.TwitterX.Actions;
using FastLane.Chatbot.TwitterX.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TwitterX.Services;

public interface ITwitterXClient : IChatbotClient
{
	/// <summary>
	/// Method for invoking message handlers
	/// </summary>
	Task PumpMessages(CancellationToken cancellationToken);

	/// <summary>
	/// Statistics from last calls of GetMessagesAsync
	/// </summary>
	public ConcurrentDictionary<string, IReadOnlyList<ChatMessage>> CurrentChatMessagesStats { get; }
}

public class TwitterXClient(
	IBrowser browser,
	ILogger<TwitterXClient> logger,
	IOptionsMonitor<Settings> settings,
	TwitterXClientsPool twitterXClientsPool)
	: ITwitterXClient, IDisposable
{
	private readonly IBrowser _browser = browser;
	private readonly ILogger<TwitterXClient> _logger = logger;
	private readonly IOptionsMonitor<Settings> _settings = settings;
	private IReadOnlyDictionary<string, int> _currentChatUnreadMessages = new Dictionary<string, int>();
	private readonly TwitterXClientsPool _twitterXClientsPool = twitterXClientsPool;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private string _botNickName;

	public ConcurrentDictionary<string, IReadOnlyList<ChatMessage>> CurrentChatMessagesStats { get; } = [];

	public event Action? MessageReceived;

	public void Dispose()
	{
		if (_twitterXClientsPool != null && _twitterXClientsPool.Count != 0)
		{
			if (_twitterXClientsPool.Count == 1)
			{ _twitterXClientsPool?.TryTake(out ITwitterXClient? _); }
			else
			{
				ITwitterXClient[] backClients = [.. _twitterXClientsPool];
				_twitterXClientsPool.Clear();

				foreach (ITwitterXClient client in backClients)
				{
					if (client != this)
					{ _twitterXClientsPool.Add(client); }
				}
			}
		}

		GC.SuppressFinalize(this);
	}

	public async ValueTask DisposeAsync()
	{
		await _browser.DisposeAsync();
		Dispose();
		GC.SuppressFinalize(this);
	}

	public async Task WaitForLoginAsync(CancellationToken cancellationToken)
	{
		try
		{
			IPage page = await GetPageAsync();
			await page.GoToAsync("https://twitter.com/messages", WaitUntilNavigation.Networkidle2);
			//await page.Keyboard.DownAsync(PuppeteerSharp.Input.Key.Shift);
			//await page.Keyboard.PressAsync(PuppeteerSharp.Input.Key.F5);
			//await page.Keyboard.UpAsync(PuppeteerSharp.Input.Key.Shift);
			// TODO: Track error of invalid caches(when some times twitter does not load contacts) and may be add Pressing Shift+F5 or smth to ignore caches

			_logger.LogInformation("Wait for enter TwitterX");
			while (!cancellationToken.IsCancellationRequested && !await IsChatsReadyAsync(cancellationToken))
			{
				await Task.Delay(1000, cancellationToken);
			}
			cancellationToken.ThrowIfCancellationRequested();
			_botNickName = await new GetBotNickName(_settings).InvokeActionAsync(_browser, cancellationToken);

			_logger.LogInformation("Entered TwitterX");
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Cancelled");
			throw;
		}
	}

	private async Task<bool> IsChatsReadyAsync(CancellationToken cancellationToken)
	{
		IPage page = await GetPageAsync();
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (await page.WaitForSelectorAsync(_settings.CurrentValue.TwitterXPageExpressions.ChatContainer) is not null)
				{
					return true;
				}
			}
			catch
			{
				await Task.Delay(1000, cancellationToken);
				return false;
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
			return await new GetUnreadMessagesStatistics(_settings).InvokeActionAsync(_browser, CurrentChatMessagesStats, cancellationToken);
		}
		catch (Exception ex)
		{
#if DEBUG
			_logger.LogError(ex, "Chatbot error");
#else
			_logger.LogError("{Message}", ex.Message);
#endif
		}
		finally { _semaphore.Release(); }

		return _currentChatUnreadMessages;
	}

	public async Task PumpMessages(CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);

		try
		{
			IReadOnlyDictionary<string, int> newStat = await new GetUnreadMessagesStatistics(_settings).InvokeActionAsync
				(_browser, CurrentChatMessagesStats, cancellationToken);

			if (MessageReceived != null && newStat.Count != 0 &&
					!(newStat == _currentChatUnreadMessages || (newStat.Count == _currentChatUnreadMessages.Count && !newStat.Except(_currentChatUnreadMessages).Any()))
					)
			{
				MessageReceived?.Invoke();
			}

			_currentChatUnreadMessages = newStat;
		}
		catch (Exception ex)
		{
#if DEBUG
			_logger.LogError(ex, "Chatbot error");
#else
			_logger.LogError("{Message}", ex.Message);
#endif
		}
		finally { _semaphore.Release(); }
	}

	public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string userId, CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);
		try
		{
			if (!await EnterChatAsync(userId, cancellationToken))
			{ throw new InvalidOperationException("Unabled to enter chat " + userId); }

			IReadOnlyList<ChatMessage> result = await new GetLastMessages(_settings).InvokeActionAsync(_browser, _botNickName, _logger, ChatMember.Bot | ChatMember.User, cancellationToken);

			CurrentChatMessagesStats[userId] = result;

			return !await CloseChatAsync(cancellationToken) ? throw new InvalidOperationException("Unabled to close active chat ") : result;
		}
		catch (Exception ex)
		{
			try
			{ await CloseChatAsync(cancellationToken); }
			catch { };
#if DEBUG
			_logger.LogError(ex, "Chatbot error");
#else
			_logger.LogError("{Message}", ex.Message);
#endif
		}
		finally
		{ _semaphore.Release(); }

		return new List<ChatMessage>();
	}

	public async Task PostAsync(string userId, string content, CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);
		try
		{
			await EnterChatAsync(userId, cancellationToken);
			await new PostMessage(_settings).InvokeActionAsync(_browser, content, cancellationToken);
			await CloseChatAsync(cancellationToken);
		}
		finally { _semaphore.Release(); }
	}

	/// <summary>
	/// Enters to specified chat
	/// </summary>
	/// <param name="chatName">Name of chat to open</param>
	/// <returns>True if operation was successfull</returns>
	public async Task<bool> EnterChatAsync(string chatName, CancellationToken cancellationToken)
	{
		return await new EnterChat(_settings).InvokeActionAsync(_browser, chatName, cancellationToken);
	}

	/// <summary>
	/// Closes currently active chat. Reloads window!
	/// </summary>
	/// <param name="cancellationToken"></param>
	public async Task<bool> CloseChatAsync(CancellationToken cancellationToken)
	{
		return await new CloseChat(_settings).InvokeActionAsync(_browser, cancellationToken);
	}
}