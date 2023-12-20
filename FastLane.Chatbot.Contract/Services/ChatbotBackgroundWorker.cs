using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Hosting;

namespace FastLane.Chatbot.Contract.Services;
internal class ChatbotBackgroundWorker : BackgroundService
{
	private readonly WhatsAppClientsPool _whatsAppClients;

	public ChatbotBackgroundWorker(WhatsAppClientsPool whatsAppClients)
	{
		_whatsAppClients = whatsAppClients;
	}

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
