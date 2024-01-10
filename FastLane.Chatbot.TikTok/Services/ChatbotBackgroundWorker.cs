using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.TikTok.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FastLane.Chatbot.TikTok.Services;
internal class ChatbotBackgroundWorker(TikTokClientsPool tiktokClients,
	ILogger<ChatbotBackgroundWorker> logger,
	IOptionsMonitor<Settings> settings) : BackgroundService
{
	private readonly TikTokClientsPool _tiktokClients = tiktokClients;
	private readonly ILogger<ChatbotBackgroundWorker> _logger = logger;
	private readonly Settings _settings = settings.CurrentValue;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			foreach (ITikTokClient client in _tiktokClients)
			{
				await client.PumpMessages(stoppingToken);
			}
			await Task.Delay(_settings.Facebook.NewStateEventPollingPeriodMs, stoppingToken);
		}

		return;
	}
}