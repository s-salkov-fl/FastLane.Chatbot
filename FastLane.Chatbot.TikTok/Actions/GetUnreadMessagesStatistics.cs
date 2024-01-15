using System.Collections.Concurrent;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.TikTok.Actions;

/// <summary>
/// Action for collect unread messages statistics
/// </summary>
public partial class GetUnreadMessagesStatistics(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;
	private readonly IOptionsMonitor<Settings> _settingsMonitor = settings;

	public async Task<IReadOnlyDictionary<string, int>> InvokeActionAsync(IBrowser browser,
		string botNickName,
		ConcurrentDictionary<string, IReadOnlyList<ChatMessage>> currentChatMessagesStats,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (browser == null)
		{ throw new ArgumentNullException(nameof(browser)); }

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.TikTokPageExpressions.ContactContainer);
		Dictionary<string, int> result = [];

		foreach (IElementHandle element in elements)
		{
			IElementHandle messagesExist = await element.QuerySelectorAsync(_settings.TikTokPageExpressions.MessageExistSign);

			if (messagesExist != null)
			{
				IElementHandle contactNameElement = await element.QuerySelectorAsync(_settings.TikTokPageExpressions.ContactName);

				if (contactNameElement != null)
				{
					string? contactName = (await contactNameElement.GetPropertyAsync("innerText")).RemoteObject?.Value.ToString();

					if (string.IsNullOrEmpty(contactName))
					{ throw new InvalidOperationException("Unable to read contact name"); }

					IElementHandle messageCountElement = await element.QuerySelectorAsync(_settings.TikTokPageExpressions.UnreadMessagesCount)
						?? throw new InvalidOperationException("Unable to find message count html element");

					string countMessage = (await messageCountElement.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();

					if (string.IsNullOrEmpty(countMessage))
					{ throw new InvalidOperationException("Unable to read messages count"); }

					int messageCount;
					if (countMessage.StartsWith("sent ", StringComparison.OrdinalIgnoreCase))
					{
						string countNum = countMessage.Replace("sent ", "", StringComparison.OrdinalIgnoreCase)
							.Replace(" messages", "", StringComparison.OrdinalIgnoreCase)
							.Replace(" message", "", StringComparison.OrdinalIgnoreCase);

						messageCount = int.Parse(countNum);
					}
					else
					{ messageCount = 1; }

					result[contactName] = messageCount;
				}
				else
				{ throw new InvalidOperationException("Unable to obtain contact name html element"); }
			}

		}

		string currentOpenedChatName = await new GetOpenedChatName(_settingsMonitor).InvokeActionAsync(browser, cancellationToken);
		bool hasOpenedChat = !string.IsNullOrEmpty(currentOpenedChatName);

		if (hasOpenedChat)
		{
			IEnumerable<string> NewMessagesStats = (await new GetLastMessages(_settingsMonitor)
				.InvokeActionAsync(browser, botNickName, ChatMember.User, cancellationToken)).Select(m => m.Content);

			if (currentChatMessagesStats.TryGetValue(currentOpenedChatName, out IReadOnlyList<ChatMessage>? oldMessages))
			{
				IEnumerable<string> oldCurChatMessages = oldMessages.Where(m => m.Member == ChatMember.User).Select(m => m.Content);

				string? lastOldMessage = oldCurChatMessages.FirstOrDefault();
				int countSameOld = oldCurChatMessages.TakeWhile(m => m == lastOldMessage).Count();

				if (lastOldMessage != null)
				{
					int countNew = NewMessagesStats.TakeWhile(s => s != lastOldMessage).Count();
					if (countNew > 0)
					{
						result[currentOpenedChatName] = countNew;
					}
					else
					{
						int countNewDups = NewMessagesStats.TakeWhile(s => s == lastOldMessage).Count();
						if (countNewDups > countSameOld)
						{ result[currentOpenedChatName] = countNewDups - countSameOld; }
					}
				}
			}
			else
			{
				result[currentOpenedChatName] = NewMessagesStats.Count();
			}

		}

		return result;
	}
}