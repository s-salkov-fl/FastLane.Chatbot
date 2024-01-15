using System.Text.RegularExpressions;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Instagram.Actions;

/// <summary>
/// Get last lessages from active opened chat
/// </summary>
public partial class GetLastMessages(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<IReadOnlyList<ChatMessage>> InvokeActionAsync(IBrowser browser, string botNickName, ChatMember memberFilter, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		return null;
		//string selector = (memberFilter != ChatMember.Bot) ? _settings.InstagramPageExpressions.ChatMessageContainerCommon
		//	 : string.Format(_settings.InstagramPageExpressions.ChatMessageContainerExactUserPattern, botNickName);

		//await page.WaitForSelectorAsync(selector);
		//IElementHandle[] elements = await page.QuerySelectorAllAsync(selector);

		//List<ChatMessage> messages = [];

		//for (int i = elements.Length; i > 0; i--)
		//{
		//	cancellationToken.ThrowIfCancellationRequested();

		//	string content = (await elements[i - 1].GetPropertyAsync("innerText")).RemoteObject.Value.ToString();

		//	IElementHandle userNameElement = await elements[i - 1].QuerySelectorAsync(_settings.InstagramPageExpressions.ChatMessageUserName)
		//		?? throw new InvalidOperationException("Unable to get sender name of message N" + i + " from end");

		//	string senderNameRaw = (await userNameElement.GetPropertyAsync("href")).RemoteObject.Value.ToString();

		//	string senderName = NickNameRegex().Match(senderNameRaw).Groups[0].Value;

		//	if (memberFilter.HasFlag(ChatMember.Bot) && senderName == botNickName)
		//	{
		//		messages.Add(new ChatMessage(content, ChatMember.Bot));
		//	}
		//	else if (memberFilter.HasFlag(ChatMember.User) && senderName != botNickName)
		//	{
		//		messages.Add(new ChatMessage(content, ChatMember.User));
		//	}
		//}

		//return messages;
	}

	[GeneratedRegex("(?<=\\@)(.*?)(?=\\?)")]
	private static partial Regex NickNameRegex();

}