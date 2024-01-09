using FastLane.Chatbot.Browser;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.WhatsApp.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.WhatsApp.Services;

/// <summary>
/// Start sequence for WhatsApp browser launch and logging into messenger
/// </summary>
public interface IWhatsAppClientFactory
{
	/// <summary>
	/// Starts browser and attempts to login into whatsapp messenger
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IWhatsAppClient> CreateClientAsync(CancellationToken cancellationToken);
}

internal class WhatsAppClientFactory(
	IBrowserFactory browserFactory,
	IServiceProvider serviceProvider,
	IOptionsMonitor<Settings> settings,
	WhatsAppClientsPool whatsAppClientsPool) : IWhatsAppClientFactory
{
	private readonly IBrowserFactory _browserFactory = browserFactory;
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly BrowserSettings _browserSettings = settings.CurrentValue.Browser;
	private readonly WhatsAppClientsPool _whatsAppClientsPool = whatsAppClientsPool;

	public async Task<IWhatsAppClient> CreateClientAsync(CancellationToken cancellationToken)
	{
		IBrowser browser = await _browserFactory.CreateBrowserAsync(_browserSettings.ProfileConfigName, cancellationToken);

		try
		{
			WhatsAppClient client = ActivatorUtilities.CreateInstance<WhatsAppClient>(_serviceProvider, browser);
			await client.WaitForLoginAsync(cancellationToken);
			_whatsAppClientsPool.Add(client);
			return client;
		}
		catch
		{
			await browser.DisposeAsync();
			throw;
		}
	}
}