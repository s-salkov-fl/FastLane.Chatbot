using System.Collections.Concurrent;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.TikTok.Actions;
using FastLane.Chatbot.TikTok.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TikTok.Services;

public interface ITikTokClient : IChatbotClient
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

public class TikTokClient(
	IBrowser browser,
	ILogger<TikTokClient> logger,
	IOptionsMonitor<Settings> settings,
	TikTokClientsPool tiktokClientsPool)
	: ITikTokClient, IDisposable
{
	private readonly IBrowser _browser = browser;
	private readonly ILogger<TikTokClient> _logger = logger;
	private readonly IOptionsMonitor<Settings> _settings = settings;
	private IReadOnlyDictionary<string, int> _currentChatUnreadMessages = new Dictionary<string, int>();
	private readonly TikTokClientsPool _tiktokClientsPool = tiktokClientsPool;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private string _botNickName;

	public ConcurrentDictionary<string, IReadOnlyList<ChatMessage>> CurrentChatMessagesStats { get; } = [];

	public event Action? MessageReceived;

	public void Dispose()
	{
		if (_tiktokClientsPool != null && _tiktokClientsPool.Count != 0)
		{
			if (_tiktokClientsPool.Count == 1)
			{ _tiktokClientsPool?.TryTake(out ITikTokClient? _); }
			else
			{
				ITikTokClient[] backClients = [.. _tiktokClientsPool];
				_tiktokClientsPool.Clear();

				foreach (ITikTokClient client in backClients)
				{
					if (client != this)
					{ _tiktokClientsPool.Add(client); }
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
			await page.GoToAsync("https://www.tiktok.com/messages?lang=en", WaitUntilNavigation.Networkidle2);

			_logger.LogInformation("Wait for enter TikTok");
			while (!cancellationToken.IsCancellationRequested && !await IsChatsReadyAsync(cancellationToken))
			{
				await Task.Delay(1000, cancellationToken);
			}

			cancellationToken.ThrowIfCancellationRequested();
			_logger.LogInformation("Entered TikTok");

			_botNickName = await new GetBotNickName(_settings).InvokeActionAsync(_browser, cancellationToken);
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
				string chatContainer = _settings.CurrentValue.TikTokPageExpressions.ChatContainer;

				cancellationToken.ThrowIfCancellationRequested();
				if (await page.WaitForSelectorAsync(chatContainer) is not null)
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
			return await new GetUnreadMessagesStatistics(_settings).InvokeActionAsync(_browser, _botNickName, CurrentChatMessagesStats, cancellationToken);
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
				(_browser, _botNickName, CurrentChatMessagesStats, cancellationToken);

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

			IReadOnlyList<ChatMessage> result = await new GetLastMessages(_settings).InvokeActionAsync(_browser, _botNickName, ChatMember.Bot | ChatMember.User, cancellationToken);

			CurrentChatMessagesStats[userId] = result;

			return result;
			//return !await CloseChatAsync(cancellationToken) ? throw new InvalidOperationException("Unabled to close active chat ") : result;
		}
		catch (Exception ex)
		{
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