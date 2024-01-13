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

		await page.WaitForSelectorAsync(_settings.TikTokPageExpressions.ContactContainer);
		IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.TikTokPageExpressions.ContactContainer);

		foreach (IElementHandle element in elements)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await element.WaitForSelectorAsync(_settings.TikTokPageExpressions.ContactName);
			IElementHandle contactNameElement = await element.QuerySelectorAsync(_settings.TikTokPageExpressions.ContactName);

			if (contactNameElement != null)
			{
				string? contactName = (await contactNameElement.GetPropertyAsync("innerText")).RemoteObject?.Value.ToString();

				if (string.IsNullOrEmpty(contactName))
				{
					await Task.Delay(_settings.TikTok.GeneralMutateFailCrutchWaitMs, cancellationToken);
					contactName = (await contactNameElement.GetPropertyAsync("innerText")).RemoteObject?.Value.ToString();
					if (string.IsNullOrEmpty(contactName))
					{ throw new InvalidOperationException("Unable to read contact name"); }
				}

				if (contactName == chatName)
				{
					cancellationToken.ThrowIfCancellationRequested();
					await contactNameElement.ClickAsync();
					await Task.Delay(_settings.TikTok.GeneralMutateFailCrutchWaitMs, cancellationToken);
					return true;
				}
			}
		}

		return false;
	}

}