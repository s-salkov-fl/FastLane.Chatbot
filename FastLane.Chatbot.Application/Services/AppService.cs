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

			IReadOnlyDictionary<string, int> oldUnreads = new Dictionary<string, int>();

			client.MessageReceived += async () =>
			{
				_logger.LogInformation("New messages event");
				IReadOnlyDictionary<string, int> unreads = await client.GetChatInboxCountAsync(stoppingToken);

				if (!((unreads == oldUnreads) || (unreads.Count == oldUnreads.Count && !unreads.Except(oldUnreads).Any())))
				{
					string outp = "New message statistics:\n";
					foreach (KeyValuePair<string, int> messageStat in unreads)
					{
						outp += $"\"{messageStat.Key}\" - {messageStat.Value} unread messages\n";
					}

					_logger.LogInformation("{Stats}", outp);

					foreach (KeyValuePair<string, int> messageStat in unreads)
					{
						IReadOnlyList<ChatMessage> messages = await client.GetMessagesAsync(messageStat.Key, stoppingToken);
						IEnumerable<ChatMessage> userNewMessages = messages.Where(m => m.Member == ChatMember.User).Take(messageStat.Value);
						_logger.LogInformation("New messages of {ChatName}:\n{Messages}", messageStat.Key, string.Join("\n", userNewMessages.Select(m => m.Content)));

						foreach (ChatMessage message in userNewMessages)
						{
							string answer = $"I do not understand this: \"{message.Content}\"";
							await client.PostAsync(messageStat.Key, answer, stoppingToken);
							_logger.LogInformation("Posted message to {ChatName} : {Message}", messageStat.Key, answer);
						}
					}
				}

				oldUnreads = unreads;
			};

			//sample get some chat correspondence
			//string chatNameInit = "Prokhor";
			//IReadOnlyList<ChatMessage> lastMessages = await client.GetMessagesAsync(chatNameInit, stoppingToken);

			//_logger.LogInformation("Last {Count} messages of {ChatName}:\n{Messages}", lastMessages.Count, chatNameInit, string.Join("\n"
			//	, lastMessages.Select(m => m.Member.ToString() + ":" + m.Content)));

			_logger.LogInformation("Monitoring new messages");

			while (!stoppingToken.IsCancellationRequested)
			{
				await Task.Delay(1000, stoppingToken);
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