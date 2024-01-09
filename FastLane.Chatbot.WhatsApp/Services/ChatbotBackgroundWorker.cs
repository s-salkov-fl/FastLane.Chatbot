using FastLane.Chatbot.WhatsApp.Model;
using Microsoft.Extensions.Hosting;

namespace FastLane.Chatbot.WhatsApp.Services;
internal class ChatbotBackgroundWorker(WhatsAppClientsPool whatsAppClients) : BackgroundService
{
	private readonly WhatsAppClientsPool _whatsAppClients = whatsAppClients;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			foreach (IWhatsAppClient client in _whatsAppClients)
			{
				await client.GetChatInboxCountAsync(stoppingToken);
			}
			await Task.Delay(1000, stoppingToken);
		}

		return;
	}
}