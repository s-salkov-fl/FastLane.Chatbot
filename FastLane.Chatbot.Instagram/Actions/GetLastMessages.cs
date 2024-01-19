using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.Contract.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Instagram.Actions;

/// <summary>
/// Get last lessages from active opened chat
/// </summary>
public partial class GetLastMessages(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<IReadOnlyList<ChatMessage>> InvokeActionAsync(IBrowser browser, string botNickName, ILogger logger, ChatMember memberFilter, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		botNickName = botNickName.NormalizeSpaces();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		await page.WaitForSelectorAsync(_settings.InstagramPageExpressions.ChatMessageContainer);
		IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.InstagramPageExpressions.ChatMessageContainer);

		List<ChatMessage> messages = [];
		//DebugBrowser.GetHtml(elements.Reverse(), null, "C:\\temp\\messages.txt");

		for (int i = elements.Length; i > 0; i--)
		{
			cancellationToken.ThrowIfCancellationRequested();

			IElementHandle userNameElement = await elements[i - 1].QuerySelectorAsync(_settings.InstagramPageExpressions.ChatMemberNameText)
				?? await elements[i - 1].QuerySelectorAsync(_settings.InstagramPageExpressions.ChatMemberNameTextAlternate)
				?? throw new InvalidOperationException($"Unable to get sender name element of message N{i}");

			string senderName = (await userNameElement.GetPropertyAsync("innerText")).RemoteObject.Value.ToString();

			if (string.IsNullOrEmpty(senderName))
			{ throw new InvalidOperationException($"For some reason SENDER NAME of message N{i} IS EMPTY"); }

			IElementHandle messageContentElement = await elements[i - 1].QuerySelectorAsync(_settings.InstagramPageExpressions.ChatMessageText)
				?? await elements[i - 1].QuerySelectorAsync(_settings.InstagramPageExpressions.ChatMessageTextAlternate)
				?? await elements[i - 1].QuerySelectorAsync(_settings.InstagramPageExpressions.ChatMessageTextAlternate2)
				?? await elements[i - 1].QuerySelectorAsync(_settings.InstagramPageExpressions.ChatMessageTextAlternate3);

			string? content = "";

			if (messageContentElement == null)
			{ logger.LogError("Unable to get text content element of message N {N}", i); }
			else
			{ content = (await messageContentElement.GetPropertyAsync("innerText"))?.RemoteObject?.Value?.ToString(); }

			if (string.IsNullOrEmpty(content?.Trim()))
			{ logger.LogError("For some reason CONTENT of message N{N} IS EMPTY", i); content = ""; }

			if (memberFilter.HasFlag(ChatMember.Bot) && string.Equals(senderName, "you sent", StringComparison.OrdinalIgnoreCase))
			{
				messages.Add(new ChatMessage(content, ChatMember.Bot));
			}
			else if (memberFilter.HasFlag(ChatMember.User) && senderName != botNickName)
			{
				messages.Add(new ChatMessage(content, ChatMember.User));
			}
		}

		return messages;
	}
}