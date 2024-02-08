using FastLane.Chatbot.WhatsAppNodeJsClient.Model;
using Microsoft.Extensions.DependencyInjection;

namespace FastLane.Chatbot.WhatsAppNodeJsClient.Services;

public interface IWhatsAppNodeJsClientFactory
{
	Task<IWhatsAppNodeJsClient> CreateClientAsync(CancellationToken cancellationToken);
}

internal class WhatsAppNodeJsClientFactory(
	IServiceProvider serviceProvider,
	WhatsAppNodeJsClientsPool whatsAppNodeJsClientsPool) : IWhatsAppNodeJsClientFactory
{
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly WhatsAppNodeJsClientsPool _whatsAppNodeJsClientsPool = whatsAppNodeJsClientsPool;

	public async Task<IWhatsAppNodeJsClient> CreateClientAsync(CancellationToken cancellationToken)
	{
		WhatsAppNodeJsClient client = ActivatorUtilities.CreateInstance<WhatsAppNodeJsClient>(_serviceProvider);
		_whatsAppNodeJsClientsPool.Add(client);
		await client.WaitForLoginAsync(cancellationToken);
		return client;
	}
}