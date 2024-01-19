using System.Text.RegularExpressions;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Utility;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Instagram.Actions;

/// <summary>
/// Finds chat name in chat list and clicks on it, i.e. enters exact chat
/// </summary>
public class EnterChat(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<bool> InvokeActionAsync(IBrowser browser, string chatName, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		chatName = chatName.NormalizeSpaces();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		IElementHandle chatContainer = await page.WaitForSelectorAsync(_settings.InstagramPageExpressions.ChatContainer);
		IElementHandle[] elements = await chatContainer.QuerySelectorAllAsync(_settings.InstagramPageExpressions.ContactContainer);

		foreach (IElementHandle element in elements)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await element.WaitForSelectorAsync(_settings.InstagramPageExpressions.ContactName);
			IElementHandle contactNameElement = await element.QuerySelectorAsync(_settings.InstagramPageExpressions.ContactName);

			if (contactNameElement != null)
			{
				string? contactName = (await contactNameElement.GetPropertyAsync("innerText")).RemoteObject?.Value.ToString();

				if (string.IsNullOrEmpty(contactName))
				{
					await Task.Delay(_settings.Instagram.GeneralMutateFailCrutchWaitMs, cancellationToken);
					contactName = (await contactNameElement.GetPropertyAsync("innerText")).RemoteObject?.Value.ToString();
					if (string.IsNullOrEmpty(contactName))
					{ throw new InvalidOperationException("Unable to read contact name"); }
				}

				if (contactName.NormalizeSpaces() == chatName)
				{
					cancellationToken.ThrowIfCancellationRequested();
					await element.ClickAsync();
					await Task.Delay(_settings.Instagram.GeneralMutateFailCrutchWaitMs, cancellationToken);
					return true;
				}
			}
		}

		return false;
	}

}