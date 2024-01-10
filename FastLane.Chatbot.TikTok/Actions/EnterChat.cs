using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TikTok.Actions;

/// <summary>
/// Finds chat name in chat list and clicks on it, i.e. enters exact chat
/// </summary>
public class EnterChat(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<bool> InvokeActionAsync(IBrowser browser, string chatName, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		//IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.TikTokPageExpressions.ContactContainer);

		//foreach (IElementHandle element in elements)
		//{
		//	IElementHandle chatIdlink = await element.QuerySelectorAsync(_settings.FaceBookPageExpressions.ChatIdLink);
		//	if (chatIdlink != null)
		//	{
		//		IElementHandle contactNameElement = await chatIdlink.QuerySelectorAsync(_settings.FaceBookPageExpressions.ContactName);

		//		if (contactNameElement != null)
		//		{
		//			string currentChatName = (await contactNameElement.GetPropertyAsync("value")).RemoteObject.Value.ToString();

		//			if (chatName == currentChatName)
		//			{
		//				IElementHandle contactLinkElement = await element.QuerySelectorAsync(_settings.FaceBookPageExpressions.ChatIdLink);
		//				await contactLinkElement.ClickAsync();
		//				await Task.Delay(_settings.Facebook.GeneralMutateFailCrutchWaitMs, cancellationToken);
		//				return true;
		//			}
		//		}
		//	}
		//}

		return false;
	}

}