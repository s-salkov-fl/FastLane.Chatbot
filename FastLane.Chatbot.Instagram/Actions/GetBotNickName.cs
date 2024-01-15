using FastLane.Chatbot.Contract.Configuration;
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
			//string content = (await botNickNameElement.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();

			//int ixBegin = content.IndexOf("\"nickName\":", StringComparison.OrdinalIgnoreCase);

			//if (ixBegin != -1)
			//{
			//	int ixStartNick = content.IndexOf(":\"", ixBegin, StringComparison.OrdinalIgnoreCase);
			//	int ixEndComma = content.IndexOf(",", ixStartNick, StringComparison.OrdinalIgnoreCase);

			//	string nickname = content[(ixStartNick + 1)..ixEndComma].Replace("\"", "");

			//	return nickname;
			//}
		}

		throw new InvalidOperationException("Unable to obtain NickName of bot from page.");
	}
}