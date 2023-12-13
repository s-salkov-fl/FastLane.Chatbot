using FastLane.Chatbot.Contract.Actions;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Contract.Services;

/// <summary>
/// WhatsApp browser routines
/// </summary>
public interface IWhatsAppClient : IAsyncDisposable
{
	/// <summary>
	/// Gets chatnames and number of unread messages
	/// </summary>
	Task<UnreadMessagesStats> GetUnreadMessagesStatsAsync(CancellationToken cancellationToken);
}

internal class WhatsAppClient : IWhatsAppClient
{
	private readonly IBrowser _browser;
	private readonly ILogger<WhatsAppClient> _logger;
	private readonly IOptionsMonitor<Settings> _settings;

	public WhatsAppClient(
		IBrowser browser,
		ILogger<WhatsAppClient> logger,
		IOptionsMonitor<Settings> settings)
	{
		_browser = browser;
		_logger = logger;
		_settings = settings;
	}

	public async ValueTask DisposeAsync()
	{
		await _browser.DisposeAsync();
	}

	public async Task WaitForLoginAsync(CancellationToken cancellationToken)
	{
		try
		{
			IPage page = await GetPageAsync();
			await page.GoToAsync("https://web.whatsapp.com", WaitUntilNavigation.Networkidle2);

			_logger.LogInformation("Wait for login");
			while (!cancellationToken.IsCancellationRequested && !await IsLoggedInAsync(cancellationToken))
			{
				await Task.Delay(1000, cancellationToken);
			}

			cancellationToken.ThrowIfCancellationRequested();
		}
		catch (OperationCanceledException)
		{
			_logger.LogWarning("Cancelled");
			throw;
		}
	}

	private async Task<bool> IsLoggedInAsync(CancellationToken cancellationToken)
	{
		IPage page = await GetPageAsync();
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				const string SearchInputXpath = "xpath///div[@contenteditable and @role='textbox']";
				if (await page.WaitForSelectorAsync(SearchInputXpath) is not null)
				{
					return true;
				}
			}
			catch
			{
				try
				{
					await Task.Delay(1000, cancellationToken);
				}
				catch
				{
					return false;
				}
			}
		}

		return false;
	}

	private async Task<IPage> GetPageAsync()
	{
		IPage[] pages = await _browser.PagesAsync();
		return pages.FirstOrDefault() ?? await _browser.NewPageAsync();
	}

	public async Task<UnreadMessagesStats> GetUnreadMessagesStatsAsync(CancellationToken cancellationToken)
	{
		return await new GetUnreadMessagesStatisticsAction(_settings).InvokeActionAsync(_browser, cancellationToken);
	}
}