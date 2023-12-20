using FastLane.Chatbot.Browser;
using FastLane.Chatbot.Browser.Configuration;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Contract.Services;

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

internal class WhatsAppClientFactory : IWhatsAppClientFactory
{
	private readonly IBrowserFactory _browserFactory;
	private readonly IServiceProvider _serviceProvider;
	private readonly BrowserSettings _browserSettings;
	private readonly WhatsAppClientsPool _whatsAppClientsPool;

	public WhatsAppClientFactory(
		IBrowserFactory browserFactory,
		IServiceProvider serviceProvider,
		IOptionsMonitor<Settings> settings,
		WhatsAppClientsPool whatsAppClientsPool)
	{
		_browserSettings = settings.CurrentValue.Browser;
		_browserFactory = browserFactory;
		_serviceProvider = serviceProvider;
		_whatsAppClientsPool = whatsAppClientsPool;
	}

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