using FastLane.Chatbot.Browser;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.TikTok.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TikTok.Services;

/// <summary>
/// Start sequence for TikTok browser launch and logging into messenger
/// </summary>
public interface ITikTokClientFactory
{
	/// <summary>
	/// Starts browser and attempts to login into TikTok messenger
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<ITikTokClient> CreateClientAsync(CancellationToken cancellationToken);
}

internal class TikTokClientFactory(
	IBrowserFactory browserFactory,
	IServiceProvider serviceProvider,
	IOptionsMonitor<Settings> settings,
	TikTokClientsPool tiktokClientsPool) : ITikTokClientFactory
{
	private readonly IBrowserFactory _browserFactory = browserFactory;
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly BrowserSettings _browserSettings = settings.CurrentValue.Browser;
	private readonly TikTokClientsPool _tiktokClientsPool = tiktokClientsPool;

	public async Task<ITikTokClient> CreateClientAsync(CancellationToken cancellationToken)
	{
		IBrowser browser = await _browserFactory.CreateBrowserAsync(_browserSettings.ProfileConfigName, cancellationToken);

		try
		{
			TikTokClient client = ActivatorUtilities.CreateInstance<TikTokClient>(_serviceProvider, browser);
			await client.WaitForLoginAsync(cancellationToken);
			_tiktokClientsPool.Add(client);
			return client;
		}
		catch
		{
			await browser.DisposeAsync();
			throw;
		}
	}
}