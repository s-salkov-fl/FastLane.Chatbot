using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.TwitterX.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FastLane.Chatbot.TwitterX.Services;
internal class ChatbotBackgroundWorker(TwitterXClientsPool twitterXClients,
	ILogger<ChatbotBackgroundWorker> logger,
	IOptionsMonitor<Settings> settings) : BackgroundService
{
	private readonly TwitterXClientsPool _twitterXClients = twitterXClients;
	private readonly ILogger<ChatbotBackgroundWorker> _logger = logger;
	private readonly Settings _settings = settings.CurrentValue;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			foreach (ITwitterXClient client in _twitterXClients)
			{
				await client.PumpMessages(stoppingToken);
			}
			await Task.Delay(_settings.TwitterX.NewStateEventPollingPeriodMs, stoppingToken);
		}

		return;
	}
}