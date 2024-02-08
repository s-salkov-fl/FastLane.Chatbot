using System.Diagnostics;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.WhatsAppNodeJsClient.Model;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FastLane.Chatbot.WhatsAppNodeJsClient.Services;

public interface IWhatsAppNodeJsClient : IChatbotClient
{
	/// <summary>
	/// Method for invoking message handlers
	/// </summary>
	Task PumpMessages(CancellationToken cancellationToken);
}

public class WhatsAppNodeJsClient(
	ILogger<WhatsAppNodeJsClient> logger,
	IOptionsMonitor<Settings> settings,
	WhatsAppNodeJsClientsPool whatsAppNodeJsClientsPool)
	: IWhatsAppNodeJsClient, IDisposable
{
	private readonly ILogger<WhatsAppNodeJsClient> _logger = logger;
	private readonly WhatsAppNodeJsChatBotSettings _whatsAppNodeJsSettings = settings.CurrentValue.WhatsAppNodeJs;
	private IReadOnlyDictionary<string, int> _currentChatUnreadMessages = new Dictionary<string, int>();
	private readonly WhatsAppNodeJsClientsPool _whatsAppNodeJsClientsPool = whatsAppNodeJsClientsPool;
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public event Action? MessageReceived;

	public void Dispose()
	{
		StaticNodeJSService.DisposeServiceProvider();
		if (_whatsAppNodeJsClientsPool != null && _whatsAppNodeJsClientsPool.Count != 0)
		{
			if (_whatsAppNodeJsClientsPool.Count == 1)
			{ _whatsAppNodeJsClientsPool?.TryTake(out IWhatsAppNodeJsClient? _); }
			else
			{
				IWhatsAppNodeJsClient[] backClients = [.. _whatsAppNodeJsClientsPool];
				_whatsAppNodeJsClientsPool.Clear();

				foreach (IWhatsAppNodeJsClient client in backClients)
				{
					if (client != this)
					{ _whatsAppNodeJsClientsPool.Add(client); }
				}
			}
		}

		GC.SuppressFinalize(this);
	}

	public ValueTask DisposeAsync()
	{
		Dispose();
		GC.SuppressFinalize(this);
		return ValueTask.CompletedTask;
	}

	public async Task WaitForLoginAsync(CancellationToken cancellationToken)
	{
		try
		{
			StaticNodeJSService.Configure<NodeJSProcessOptions>(options =>
			{
				options.ProjectPath = _whatsAppNodeJsSettings.NodeJsApplicationPath;
			});

			string? lastQr = "";

			while (!cancellationToken.IsCancellationRequested)
			{
				WhatsAppAuthResult? result = await StaticNodeJSService.InvokeFromFileAsync<WhatsAppAuthResult>("app.js", "authenticateStatus", cancellationToken: cancellationToken);

				if (result?.IsReady != null && result.IsReady == true)
				{ break; }
				else if (!string.IsNullOrEmpty(result?.LastQr) && result.LastQr != lastQr)
				{
					_logger.LogInformation("QrCode required to authenticate:\n{QrCode}", result.LastQr);
					lastQr = result.LastQr;
				}

				await Task.Delay(_whatsAppNodeJsSettings.AuthQrPollIntervalMs, cancellationToken);
			}

			_logger.LogInformation("Authenticated whatsApp through NodeJs successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError("Unable to enter WhatsApp:\n{Exception}", ex);
		}
	}

	private async Task<bool> WaitReadyClientAsync(CancellationToken cancellationToken)
	{
		Stopwatch watch = Stopwatch.StartNew();

		while (!cancellationToken.IsCancellationRequested)
		{
			bool isReady = await StaticNodeJSService.InvokeFromFileAsync<bool>("app.js", "isReadyClient", cancellationToken: cancellationToken);

			if (isReady)
			{
				return true;
			}
			else if (watch.ElapsedMilliseconds > _whatsAppNodeJsSettings.IsReadyClientFailPeriodMs)
			{
				throw new TimeoutException("NodeJs client was not ready within " + _whatsAppNodeJsSettings.IsReadyClientFailPeriodMs + " milliseconds!");
			}

			await Task.Delay(_whatsAppNodeJsSettings.AuthQrPollIntervalMs, cancellationToken);
		}

		return false;
	}

	public async Task PumpMessages(CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);

		try
		{
			await WaitReadyClientAsync(cancellationToken);

			KeyValuePair<string, int>[]? unreadCountStats = await StaticNodeJSService.InvokeFromFileAsync<KeyValuePair<string, int>[]?>("app.js", "getChatUnreads", cancellationToken: cancellationToken) ?? [];

			Dictionary<string, int> newStat = new(unreadCountStats);

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

	public async Task<IReadOnlyDictionary<string, int>> GetChatInboxCountAsync(CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);
		try
		{
			await WaitReadyClientAsync(cancellationToken);
			KeyValuePair<string, int>[]? unreadCountStats = await StaticNodeJSService.InvokeFromFileAsync<KeyValuePair<string, int>[]?>("app.js", "getChatUnreads", cancellationToken: cancellationToken) ?? [];

			Dictionary<string, int> newStat = new(unreadCountStats);
			return newStat;
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

	public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string userId, CancellationToken cancellationToken)
	{
		await _semaphore.WaitAsync(cancellationToken);
		try
		{
			await WaitReadyClientAsync(cancellationToken);
			ChatMessage[] result = await StaticNodeJSService.InvokeFromFileAsync<ChatMessage[]>("app.js", "getMessages", args: [userId], cancellationToken: cancellationToken) ?? [];
			return new List<ChatMessage>(result);
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
			await WaitReadyClientAsync(cancellationToken);
			bool result = await StaticNodeJSService.InvokeFromFileAsync<bool>("app.js", "postMessage", args: [userId, content], cancellationToken: cancellationToken);

		}
		finally { _semaphore.Release(); }
	}

}