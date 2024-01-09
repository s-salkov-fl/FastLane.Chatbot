using FastLane.Chatbot.Browser;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Facebook.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Facebook.Services;

/// <summary>
/// Start sequence for Facebook browser launch and logging into messenger
/// </summary>
public interface IFacebookClientFactory
{
	/// <summary>
	/// Starts browser and attempts to login into facebook messenger
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IFacebookClient> CreateClientAsync(CancellationToken cancellationToken);
}

internal class FacebookClientFactory(
	IBrowserFactory browserFactory,
	IServiceProvider serviceProvider,
	IOptionsMonitor<Settings> settings,
	FacebookClientsPool facebookClientsPool) : IFacebookClientFactory
{
	private readonly IBrowserFactory _browserFactory = browserFactory;
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly BrowserSettings _browserSettings = settings.CurrentValue.Browser;
	private readonly FacebookClientsPool _facebookClientsPool = facebookClientsPool;

	public async Task<IFacebookClient> CreateClientAsync(CancellationToken cancellationToken)
	{
		IBrowser browser = await _browserFactory.CreateBrowserAsync(_browserSettings.ProfileConfigName, cancellationToken);

		try
		{
			FacebookClient client = ActivatorUtilities.CreateInstance<FacebookClient>(_serviceProvider, browser);
			await client.WaitForLoginAsync(cancellationToken);
			_facebookClientsPool.Add(client);
			return client;
		}
		catch
		{
			await browser.DisposeAsync();
			throw;
		}
	}
}