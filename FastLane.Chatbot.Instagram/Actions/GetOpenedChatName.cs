using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Instagram.Actions;

/// <summary>
/// Returns current opened chatname
/// </summary>
public class GetOpenedChatName(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<string> InvokeActionAsync(IBrowser browser, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		throw new NotImplementedException();
		//IElementHandle elementChatActiveName = await page.QuerySelectorAsync(_settings.InstagramPageExpressions.OpenedChatHeaderNickName);

		//if (elementChatActiveName != null)
		//{
		//	string? contactName = (await elementChatActiveName.GetPropertyAsync("innerText")).RemoteObject?.Value.ToString();

		//	return string.IsNullOrEmpty(contactName)
		//		? throw new InvalidOperationException("Unable to read current active chat name")
		//		: contactName;
		//}

		return "";
	}

}