using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.Contract.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.Contract;

public static class Injections
{
	public static IServiceCollection AddWhatsAppChatbot(this IServiceCollection services)
	{
		return services
			.AddSingleton<WhatsAppClientsPool>()
			.AddSingleton<IWhatsAppClientFactory, WhatsAppClientFactory>();
	}

	public static IServiceCollection AddChatbot(this IServiceCollection services)
	{
		return
			services
			.AddHostedService<ChatbotBackgroundWorker>();
	}
}