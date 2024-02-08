using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.WhatsAppNodeJsClient.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FastLane.Chatbot.WhatsAppNodeJsClient.Services;
internal class ChatbotBackgroundWorker(WhatsAppNodeJsClientsPool whatsAppNodeJsClients, IOptionsMonitor<Settings> settings) : BackgroundService
{
	private readonly WhatsAppNodeJsClientsPool _whatsAppNodeJsClients = whatsAppNodeJsClients;
	private readonly Settings _settings = settings.CurrentValue;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			foreach (IWhatsAppNodeJsClient client in _whatsAppNodeJsClients)
			{
				await client.PumpMessages(stoppingToken);
			}
			await Task.Delay(_settings.WhatsAppNodeJs.NewStateEventPollingPeriodMs, stoppingToken);
		}
		return;
	}
}