using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.Contract.Services;

namespace FastLane.Chatbot.Application.Services;

public class AppService : BackgroundService
{
	private readonly ILogger<AppService> _logger;
	private readonly IWhatsAppClientFactory _clientFactory;
	private readonly IServiceProvider _provider;

	public AppService(ILogger<AppService> logger,
		IWhatsAppClientFactory clientFactory, IServiceProvider provider)
	{
		_logger = logger;
		_clientFactory = clientFactory;
		_provider = provider;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			_logger.LogInformation("Create the Messenger client");
			await using IWhatsAppClient client = await _clientFactory.CreateClientAsync(stoppingToken);

			_logger.LogInformation("Monitoring new messages");
			UnreadMessagesStats oldStats = null;

			while (!stoppingToken.IsCancellationRequested)
			{
				await Task.Delay(1000, stoppingToken);
				UnreadMessagesStats unreadStats = await client.GetUnreadMessagesStatsAsync(stoppingToken);

				if (!unreadStats.Equals(oldStats) && unreadStats.Messages.Count != 0)
				{
					string outp = "New message statistics:\n";
					foreach (KeyValuePair<string, int> messageStat in unreadStats.Messages)
					{
						outp += $"\"{messageStat.Key}\" - {messageStat.Value} unread messages\n";
					}

					_logger.LogInformation("{Stats}", outp);

					foreach (KeyValuePair<string, int> messageStat in unreadStats.Messages)
					{
						await client.EnterChatAsync(messageStat.Key, stoppingToken);
						List<string> messages = await client.GetLastMessagesAsync(messageStat.Key, messageStat.Value, stoppingToken);
						_logger.LogInformation("New messages of {ChatName}:\n{Messages}", messageStat.Key, string.Join("\n", messages));

						foreach (string message in messages)
						{
							string answer = $"I do not understand this: \"{message}\"";
							await client.PostMessage(answer, stoppingToken);
							_logger.LogInformation("Posted message to {ChatName} : {Message}", messageStat.Key, answer);
						}

						await client.CloseChatAsync(stoppingToken);
					}

				}

				oldStats = unreadStats;
			}
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("Stopped");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Chatbot error");
		}

	}
}