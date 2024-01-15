using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.Facebook.Services;
using FastLane.Chatbot.WhatsApp.Services;
using FastLane.Chatbot.Instagram.Services;
using FastLane.Chatbot.TikTok.Services;

namespace FastLane.Chatbot.Application.Services;

public class AppService(
	ILogger<AppService> logger,
	IWhatsAppClientFactory whatsAppClientFactory,
	IFacebookClientFactory facebookClientFactory,
	ITikTokClientFactory tiktokClientFactory,
	IInstagramClientFactory instagramClientFactory
	) : BackgroundService
{
	private readonly ILogger<AppService> _logger = logger;
	private readonly IWhatsAppClientFactory _whatsAppClientFactory = whatsAppClientFactory;
	private readonly IFacebookClientFactory _facebookClientFactory = facebookClientFactory;
	private readonly ITikTokClientFactory _tiktokClientFactory = tiktokClientFactory;
	private readonly IInstagramClientFactory _instagramClientFactory = instagramClientFactory;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			//await RunWhatsAppAsync(stoppingToken).ConfigureAwait(false);
			//await RunFacebookAsync(stoppingToken).ConfigureAwait(false);
			//await RunTikTokAsync(stoppingToken).ConfigureAwait(false);
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

	public async Task RunWhatsAppAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Create the WhatsApp client");
		await using IWhatsAppClient client = await _whatsAppClientFactory.CreateClientAsync(stoppingToken);

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

	public async Task RunFacebookAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Create the Facebook client");
		await using IFacebookClient client = await _facebookClientFactory.CreateClientAsync(stoppingToken);

		client.MessageReceived += async () =>
		{
			_logger.LogInformation("New messages event");
			Dictionary<string, int> unreads = (Dictionary<string, int>)await client.GetChatInboxCountAsync(stoppingToken);

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
		};

		_logger.LogInformation("Monitoring new messages");

		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(1000, stoppingToken);
		}
	}

	public async Task RunTikTokAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Create the TikTok client");
		await using ITikTokClient client = await _tiktokClientFactory.CreateClientAsync(stoppingToken);

		client.MessageReceived += async () =>
		{
			_logger.LogInformation("New messages event");
			Dictionary<string, int> unreads = (Dictionary<string, int>)await client.GetChatInboxCountAsync(stoppingToken);

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
		};

		////sample get some chat correspondence
		//string chatNameInit = "samir_and_aly";
		//IReadOnlyList<ChatMessage> lastMessages = await client.GetMessagesAsync(chatNameInit, stoppingToken);

		//_logger.LogInformation("Last {Count} messages of {ChatName}:\n{Messages}", lastMessages.Count, chatNameInit, string.Join("\n"
		//	, lastMessages.Select(m => m.Member.ToString() + ":" + m.Content)));

		_logger.LogInformation("Monitoring new messages");

		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(1000, stoppingToken);
		}
	}

	public async Task RunInstagramAsync(CancellationToken stoppingToken)
	{
		_logger.LogInformation("Create the Instagram client");
		await using IInstagramClient client = await _instagramClientFactory.CreateClientAsync(stoppingToken);

		client.MessageReceived += async () =>
		{
			_logger.LogInformation("New messages event");
			Dictionary<string, int> unreads = (Dictionary<string, int>)await client.GetChatInboxCountAsync(stoppingToken);

			string outp = "New message statistics:\n";
			foreach (KeyValuePair<string, int> messageStat in unreads)
			{
				outp += $"\"{messageStat.Key}\" - {messageStat.Value} unread messages\n";
			}

			_logger.LogInformation("{Stats}", outp);

			//foreach (KeyValuePair<string, int> messageStat in unreads)
			//{
			//	IReadOnlyList<ChatMessage> messages = await client.GetMessagesAsync(messageStat.Key, stoppingToken);
			//	IEnumerable<ChatMessage> userNewMessages = messages.Where(m => m.Member == ChatMember.User).Take(messageStat.Value);
			//	_logger.LogInformation("New messages of {ChatName}:\n{Messages}", messageStat.Key, string.Join("\n", userNewMessages.Select(m => m.Content)));

			//	foreach (ChatMessage message in userNewMessages)
			//	{
			//		string answer = $"I do not understand this: \"{message.Content}\"";
			//		await client.PostAsync(messageStat.Key, answer, stoppingToken);
			//		_logger.LogInformation("Posted message to {ChatName} : {Message}", messageStat.Key, answer);
			//	}
			//}
		};

		////sample get some chat correspondence
		//string chatNameInit = "samir_and_aly";
		//IReadOnlyList<ChatMessage> lastMessages = await client.GetMessagesAsync(chatNameInit, stoppingToken);

		//_logger.LogInformation("Last {Count} messages of {ChatName}:\n{Messages}", lastMessages.Count, chatNameInit, string.Join("\n"
		//	, lastMessages.Select(m => m.Member.ToString() + ":" + m.Content)));

		_logger.LogInformation("Monitoring new messages");

		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(1000, stoppingToken);
		}
	}
}