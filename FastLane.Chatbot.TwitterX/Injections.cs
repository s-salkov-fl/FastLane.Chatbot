using FastLane.Chatbot.TwitterX.Model;
using FastLane.Chatbot.TwitterX.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.TwitterX;

public static class Injections
{
	public static IServiceCollection AddTwitterXChatbot(this IServiceCollection services)
	{
		return services
			.AddSingleton<TwitterXClientsPool>()
			.AddSingleton<ITwitterXClientFactory, TwitterXClientFactory>();
	}

	public static IServiceCollection AddTwitterX(this IServiceCollection services)
	{
		return
			services
			.AddHostedService<ChatbotBackgroundWorker>();
	}
}