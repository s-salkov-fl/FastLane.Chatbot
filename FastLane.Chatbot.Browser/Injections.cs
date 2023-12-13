using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.Browser;

public static class Injections
{
	public static IServiceCollection AddBrowser(this IServiceCollection services)
	{
		return services
			.AddSingleton<IBrowserFactory, BrowserFactory>();
	}
}