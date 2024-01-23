using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TwitterX.Actions;

/// <summary>
/// Action for get BotNick name from chat page markup
/// </summary>
public partial class GetBotNickName(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<string> InvokeActionAsync(IBrowser browser, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (browser == null)
		{ throw new ArgumentNullException(nameof(browser)); }

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		//File.WriteAllText("C:\\temp\\page.txt", await page.GetContentAsync());

		IElementHandle botNickNameElement = await page.WaitForSelectorAsync(_settings.TwitterXPageExpressions.BotNickName);
		if (botNickNameElement != null)
		{
			string initalScriptText = (await botNickNameElement.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();

			int startMarker = initalScriptText.IndexOf("window.__INITIAL_STATE__");

			if (startMarker == -1)
			{ throw new InvalidOperationException("Unable to find start search marker in script 'window.__INITIAL_STATE__'"); }

			int startName = initalScriptText.IndexOf("\"name\":", startMarker);

			if (startMarker == -1)
			{ throw new InvalidOperationException("Unable to find 'name:' value in script"); }

			int openQuote = initalScriptText.IndexOf('\"', startName + 7);
			int closeQuote = initalScriptText.IndexOf('\"', openQuote + 1);

			if (openQuote == -1 || closeQuote == -1 || closeQuote <= openQuote)
			{ throw new InvalidOperationException("Unable to find position on bot name after finding 'name:' value in script"); }

			string botName = initalScriptText[(openQuote + 1)..closeQuote];

			return botName;
		}

		throw new InvalidOperationException("Unable to obtain start element for search Name of bot from page.");
	}
}