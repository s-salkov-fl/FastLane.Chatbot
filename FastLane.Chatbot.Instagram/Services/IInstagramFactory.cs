using FastLane.Chatbot.Browser;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Instagram.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Instagram.Services;

/// <summary>
/// Start sequence for Instagram browser launch and logging into messenger
/// </summary>
public interface IInstagramClientFactory
{
	/// <summary>
	/// Starts browser and attempts to login into Instagram messenger
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IInstagramClient> CreateClientAsync(CancellationToken cancellationToken);
}

internal class InstagramClientFactory(
	IBrowserFactory browserFactory,
	IServiceProvider serviceProvider,
	IOptionsMonitor<Settings> settings,
	InstagramClientsPool instagramClientsPool) : IInstagramClientFactory
{
	private readonly IBrowserFactory _browserFactory = browserFactory;
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly BrowserSettings _browserSettings = settings.CurrentValue.Browser;
	private readonly InstagramClientsPool _instagramClientsPool = instagramClientsPool;

	public async Task<IInstagramClient> CreateClientAsync(CancellationToken cancellationToken)
	{
		IBrowser browser = await _browserFactory.CreateBrowserAsync(_browserSettings.ProfileConfigName, cancellationToken);

		try
		{
			InstagramClient client = ActivatorUtilities.CreateInstance<InstagramClient>(_serviceProvider, browser);
			await client.WaitForLoginAsync(cancellationToken);
			_instagramClientsPool.Add(client);
			return client;
		}
		catch
		{
			await browser.DisposeAsync();
			throw;
		}
	}
}