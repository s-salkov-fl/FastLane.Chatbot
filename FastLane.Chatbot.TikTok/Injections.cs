using FastLane.Chatbot.TikTok.Model;
using FastLane.Chatbot.TikTok.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.TikTok;

public static class Injections
{
	public static IServiceCollection AddTikTokChatbot(this IServiceCollection services)
	{
		return services
			.AddSingleton<TikTokClientsPool>()
			.AddSingleton<ITikTokClientFactory, TikTokClientFactory>();
	}

	public static IServiceCollection AddTikTok(this IServiceCollection services)
	{
		return
			services
			.AddHostedService<ChatbotBackgroundWorker>();
	}
}