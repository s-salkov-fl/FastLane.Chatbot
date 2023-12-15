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
	/// Gets chatnames and number of unread messages
	/// </summary>
	Task<UnreadMessagesStats> GetUnreadMessagesStatsAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Filter chat list by typing text into Search box
	/// </summary>
	/// <param name="searchText">Chat name piece to enter into search box</param>
	Task<object> FilterChatsAsync(string searchText, CancellationToken cancellationToken);

	/// <summary>
	/// Enters to specified chat
	/// </summary>
	/// <param name="chatName">Name of chat to open</param>
	/// <returns>True if operation was successfull</returns>
	Task<bool> EnterChatAsync(string chatName, CancellationToken cancellationToken);

	/// <summary>
	/// Closes currently active chat
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<bool> CloseChatAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Get last messages of chat with given name
	/// </summary>
	Task<List<string>> GetLastMessagesAsync(string chatName, int messagesCount, CancellationToken cancellationToken);

	/// <summary>
	/// Send message to active chat
	/// </summary>
	Task<bool> PostMessage(string message, CancellationToken cancellationToken);
}

internal class WhatsAppClient : IWhatsAppClient
{
	private readonly IBrowser _browser;
	private readonly ILogger<WhatsAppClient> _logger;
	private readonly IOptionsMonitor<Settings> _settings;

	public WhatsAppClient(
		IBrowser browser,
		ILogger<WhatsAppClient> logger,
		IOptionsMonitor<Settings> settings)
	{
		_browser = browser;
		_logger = logger;
		_settings = settings;
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

	public async Task<UnreadMessagesStats> GetUnreadMessagesStatsAsync(CancellationToken cancellationToken)
	{
		return await new GetUnreadMessagesStatisticsAction(_settings).InvokeActionAsync(_browser, cancellationToken);
	}

	public async Task<object> FilterChatsAsync(string searchText, CancellationToken cancellationToken)
	{
		return await new InputSearchBoxAction(_settings).InvokeActionAsync((_browser, searchText), cancellationToken);
	}

	public async Task<bool> EnterChatAsync(string chatName, CancellationToken cancellationToken)
	{
		return await new EnterChatAction(_settings).InvokeActionAsync((_browser, chatName), cancellationToken);
	}

	public async Task<bool> CloseChatAsync(CancellationToken cancellationToken)
	{
		return await new CloseChatAction(_settings).InvokeActionAsync(_browser, cancellationToken);
	}

	public async Task<List<string>> GetLastMessagesAsync(string chatName, int messagesCount, CancellationToken cancellationToken)
	{
		return await new GetLastMessagesAction(_settings).InvokeActionAsync((_browser, chatName, messagesCount), cancellationToken);
	}

	public async Task<List<string>> GetNewMessagesAsync(string chatName, CancellationToken cancellationToken)
	{
		await FilterChatsAsync("", cancellationToken);
		UnreadMessagesStats unreads = await GetUnreadMessagesStatsAsync(cancellationToken);

		foreach (KeyValuePair<string, int> unreadChatStat in unreads.Messages)
		{
			if (unreadChatStat.Key == chatName)
			{
				return !await new EnterChatAction(_settings).InvokeActionAsync((_browser, chatName), cancellationToken)
					? throw new InvalidDataException("Unable to open chat - \"" + chatName + "\"")
					: await new GetLastMessagesAction(_settings).InvokeActionAsync((_browser, chatName, unreadChatStat.Value), cancellationToken);
			}
		}

		return new List<string>();
	}

	public async Task<bool> PostMessage(string message, CancellationToken cancellationToken)
	{
		return await new PostMessageAction(_settings).InvokeActionAsync((_browser, message), cancellationToken);
	}
}