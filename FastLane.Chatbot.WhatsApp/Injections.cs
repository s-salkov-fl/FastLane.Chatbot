using FastLane.Chatbot.WhatsApp.Model;
using FastLane.Chatbot.WhatsApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.WhatsApp;

public static class Injections
{
	public static IServiceCollection AddWhatsAppChatbot(this IServiceCollection services)
	{
		return services
			.AddSingleton<WhatsAppClientsPool>()
			.AddSingleton<IWhatsAppClientFactory, WhatsAppClientFactory>();
	}

	public static IServiceCollection AddWhatsApp(this IServiceCollection services)
	{
		return
			services
			.AddHostedService<ChatbotBackgroundWorker>();
	}
}