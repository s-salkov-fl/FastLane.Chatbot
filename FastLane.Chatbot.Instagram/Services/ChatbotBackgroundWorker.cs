using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Instagram.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FastLane.Chatbot.Instagram.Services;
internal class ChatbotBackgroundWorker(InstagramClientsPool instagramClients,
	ILogger<ChatbotBackgroundWorker> logger,
	IOptionsMonitor<Settings> settings) : BackgroundService
{
	private readonly InstagramClientsPool _instagramClients = instagramClients;
	private readonly ILogger<ChatbotBackgroundWorker> _logger = logger;
	private readonly Settings _settings = settings.CurrentValue;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			foreach (IInstagramClient client in _instagramClients)
			{
				await client.PumpMessages(stoppingToken);
			}
			await Task.Delay(_settings.Instagram.NewStateEventPollingPeriodMs, stoppingToken);
		}

		return;
	}
}