using System.Collections.Concurrent;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.Contract.Utility;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TwitterX.Actions;

/// <summary>
/// Action for collect unread messages statistics
/// </summary>
public partial class GetUnreadMessagesStatistics(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;
	private readonly IOptionsMonitor<Settings> _settingsMonitor = settings;

	public async Task<IReadOnlyDictionary<string, int>> InvokeActionAsync(IBrowser browser,
		ConcurrentDictionary<string, IReadOnlyList<ChatMessage>> currentChatMessagesStats,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (browser == null)
		{ throw new ArgumentNullException(nameof(browser)); }

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		IElementHandle chatContainer = await page.WaitForSelectorAsync(_settings.TwitterXPageExpressions.ChatContainer);
		IElementHandle[] elements = await chatContainer.QuerySelectorAllAsync(_settings.TwitterXPageExpressions.ContactContainer);
		Dictionary<string, int> result = [];

		//File.WriteAllText("C:\\temp\\page.txt", await page.GetContentAsync());
		//Browser.Utility.DebugBrowser.GetHtml([chatContainer], null, "C:\\temp\\chat.txt");
		//Browser.Utility.DebugBrowser.GetHtml(elements, null, "C:\\temp\\contacts.txt");

		foreach (IElementHandle element in elements)
		{
			IElementHandle[] messagesExist = await element.QuerySelectorAllAsync(_settings.TwitterXPageExpressions.MessageExistSign);

			if (messagesExist != null && messagesExist.Length == 3)
			{
				IElementHandle contactNameElement = await element.QuerySelectorAsync(_settings.TwitterXPageExpressions.ContactName);

				if (contactNameElement != null)
				{
					string? contactName = (await contactNameElement.GetPropertyAsync("innerText")).RemoteObject?.Value.ToString();

					if (string.IsNullOrEmpty(contactName))
					{ throw new InvalidOperationException("Unable to read contact name"); }

					contactName = contactName.NormalizeSpaces();

					result[contactName] = 1;
				}
				else
				{ throw new InvalidOperationException("Unable to obtain contact name html element"); }
			}

		}

		return result;
	}
}