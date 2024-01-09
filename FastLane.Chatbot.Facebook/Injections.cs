using FastLane.Chatbot.Facebook.Model;
using FastLane.Chatbot.Facebook.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.Facebook;

public static class Injections
{
	public static IServiceCollection AddFacebookChatbot(this IServiceCollection services)
	{
		return services
			.AddSingleton<FacebookClientsPool>()
			.AddSingleton<IFacebookClientFactory, FacebookClientFactory>();
	}

	public static IServiceCollection AddFacebook(this IServiceCollection services)
	{
		return
			services
			.AddHostedService<ChatbotBackgroundWorker>();
	}
}