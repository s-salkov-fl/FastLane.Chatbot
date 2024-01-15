using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Instagram.Actions;

/// <summary>
/// Closes currently opened chat
/// </summary>
public class CloseChat(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<bool> InvokeActionAsync(IBrowser argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (argument == null)
		{ throw new ArgumentNullException(nameof(argument)); }

		IBrowser browser = argument;

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		await page.EvaluateExpressionAsync("history.go(-(history.length-2))");
		await Task.Delay(100, cancellationToken);

		return true;
	}

}