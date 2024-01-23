using FastLane.Chatbot.Browser;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.TwitterX.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TwitterX.Services;

/// <summary>
/// Start sequence for TwitterX browser launch and logging into messenger
/// </summary>
public interface ITwitterXClientFactory
{
	/// <summary>
	/// Starts browser and attempts to login into TwitterX messenger
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<ITwitterXClient> CreateClientAsync(CancellationToken cancellationToken);
}

internal class TwitterXClientFactory(
	IBrowserFactory browserFactory,
	IServiceProvider serviceProvider,
	IOptionsMonitor<Settings> settings,
	TwitterXClientsPool twitterXClientsPool) : ITwitterXClientFactory
{
	private readonly IBrowserFactory _browserFactory = browserFactory;
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly BrowserSettings _browserSettings = settings.CurrentValue.Browser;
	private readonly TwitterXClientsPool _twitterXClientsPool = twitterXClientsPool;

	public async Task<ITwitterXClient> CreateClientAsync(CancellationToken cancellationToken)
	{
		IBrowser browser = await _browserFactory.CreateBrowserAsync(_browserSettings.ProfileConfigName, cancellationToken);

		try
		{
			TwitterXClient client = ActivatorUtilities.CreateInstance<TwitterXClient>(_serviceProvider, browser);

			await client.WaitForLoginAsync(cancellationToken);

			_twitterXClientsPool.Add(client);
			return client;
		}
		catch
		{
			await browser.DisposeAsync();
			throw;
		}
	}
}