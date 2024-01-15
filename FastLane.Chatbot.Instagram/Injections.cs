using FastLane.Chatbot.Instagram.Model;
using FastLane.Chatbot.Instagram.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.Instagram;

public static class Injections
{
	public static IServiceCollection AddInstagramChatbot(this IServiceCollection services)
	{
		return services
			.AddSingleton<InstagramClientsPool>()
			.AddSingleton<IInstagramClientFactory, InstagramClientFactory>();
	}

	public static IServiceCollection AddInstagram(this IServiceCollection services)
	{
		return
			services
			.AddHostedService<ChatbotBackgroundWorker>();
	}
}