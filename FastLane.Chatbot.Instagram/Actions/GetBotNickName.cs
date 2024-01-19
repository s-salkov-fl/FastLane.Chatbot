using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Utility;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Instagram.Actions;

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

		IElementHandle botNickNameElement = await page.QuerySelectorAsync(_settings.InstagramPageExpressions.BotNickName);
		if (botNickNameElement != null)
		{
			string nickName = (await botNickNameElement.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();

			return nickName.NormalizeSpaces();
		}

		throw new InvalidOperationException("Unable to obtain NickName of bot from page.");
	}
}