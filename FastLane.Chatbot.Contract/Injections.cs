using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.Contract.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.Contract;

public static class Injections
{
	public static IServiceCollection AddWhatsAppChatbot(this IServiceCollection services)
	{
		IEnumerable<Type> reflTransTypeFounded = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
			.Where(t => t.GetInterface(nameof(IAction)) != null && !t.IsAbstract);

		foreach (Type refltype in reflTransTypeFounded)
		{
			services.AddTransient(refltype);
		}

		return services
			.AddSingleton<IWhatsAppClientFactory, WhatsAppClientFactory>();
	}
}