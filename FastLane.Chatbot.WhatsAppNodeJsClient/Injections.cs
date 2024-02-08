using FastLane.Chatbot.WhatsAppNodeJsClient.Model;
using FastLane.Chatbot.WhatsAppNodeJsClient.Services;
using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.WhatsAppNodeJsClient;

public static class Injections
{
	public static IServiceCollection AddWhatsAppNodeJsChatbot(this IServiceCollection services)
	{
		return services.AddNodeJS()
			.AddSingleton<WhatsAppNodeJsClientsPool>()
			.AddSingleton<IWhatsAppNodeJsClientFactory, WhatsAppNodeJsClientFactory>();
	}

	public static IServiceCollection AddWhatsAppNodeJs(this IServiceCollection services)
	{
		return
			services
			.AddHostedService<ChatbotBackgroundWorker>();
	}
}