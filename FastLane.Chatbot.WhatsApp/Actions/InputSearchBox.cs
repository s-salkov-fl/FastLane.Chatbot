using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.WhatsApp.Actions;

/// <summary>
/// Action for type some search text into Search text box
/// </summary>
public class InputSearchBox(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<bool> InvokeActionAsync((IBrowser browser, string text) argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		(IBrowser browser, string text) = argument;

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		string SearchInputXpath = _settings.WhatsAppPageExpressions.SearchInputXpath;
		IElementHandle searchInput = await page.WaitForSelectorAsync(SearchInputXpath);

		string curValue = await searchInput.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

		if (curValue.Replace("\n", "").Length != 0)
		{
			for (int i = 0; i < curValue.Length; i++)
			{
				await searchInput.PressAsync("Backspace");
				await Task.Delay(100, cancellationToken);
			}
		}

		if (!string.IsNullOrEmpty(text))
		{
			await searchInput.TypeAsync(text);
			await Task.Delay(1000, cancellationToken);
		}

		return true;
	}

}