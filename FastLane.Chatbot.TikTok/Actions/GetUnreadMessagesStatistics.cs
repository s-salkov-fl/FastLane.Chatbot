using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TikTok.Actions;

/// <summary>
/// Action for collect unread messages statistics
/// </summary>
public partial class GetUnreadMessagesStatistics(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<IReadOnlyDictionary<string, int>> InvokeActionAsync(IBrowser browser, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (browser == null)
		{ throw new ArgumentNullException(nameof(browser)); }

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.TikTokPageExpressions.ContactContainer);
		Dictionary<string, int> result = [];

		//foreach (IElementHandle element in elements)
		//{
		//	IElementHandle chatIdlink = await element.QuerySelectorAsync(_settings.FaceBookPageExpressions.ChatIdLink);
		//	if (chatIdlink != null)
		//	{
		//		IElementHandle messagesExist = await chatIdlink.QuerySelectorAsync(_settings.FaceBookPageExpressions.MessageExistSign);

		//		if (messagesExist != null)
		//		{
		//			IElementHandle contactNameElement = await element.QuerySelectorAsync(_settings.FaceBookPageExpressions.ContactName);
		//			string chatName = (await contactNameElement.GetPropertyAsync("value")).RemoteObject.Value.ToString();

		//			result[chatName] = 1;
		//		}
		//	}
		//}

		return result;
	}

	//[GeneratedRegex("[^0-9]")]
	//private static partial Regex NumRegex();
}