using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Facebook.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FastLane.Chatbot.Facebook.Services;
internal class ChatbotBackgroundWorker(FacebookClientsPool facebookClients,
	ILogger<ChatbotBackgroundWorker> logger,
	IOptionsMonitor<Settings> settings) : BackgroundService
{
	private readonly FacebookClientsPool _facebookClients = facebookClients;
	private readonly ILogger<ChatbotBackgroundWorker> _logger = logger;
	private readonly Settings _settings = settings.CurrentValue;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			foreach (IFacebookClient client in _facebookClients)
			{
				await client.PumpMessages(stoppingToken);
			}
			await Task.Delay(_settings.Facebook.NewStateEventPollingPeriodMs, stoppingToken);
		}

		return;
	}
}