using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.WhatsApp.Actions;

/// <summary>
/// Get last lessages from chat
/// </summary>
public class GetLastMessages(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<IReadOnlyList<ChatMessage>> InvokeActionAsync(IBrowser browser, ChatMember memberFilter, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		string neededFilter = memberFilter.HasFlag(ChatMember.User | ChatMember.Bot) ? _settings.WhatsAppPageExpressions.UserAndBotCommentBoxXpath
			: memberFilter.HasFlag(ChatMember.Bot) ? _settings.WhatsAppPageExpressions.BotCommentBoxXpath : _settings.WhatsAppPageExpressions.UserCommentBoxXpath;

		IElementHandle[] elements = await page.QuerySelectorAllAsync(neededFilter);

		List<ChatMessage> messages = [];
		for (int i = elements.Length; i > 0; i--)
		{
			IElementHandle curElement = elements[i - 1];

			//IElementHandle elementTypeUserMarker = await curElement.QuerySelectorAsync(_settings.PageExpressions.UserMessageMarker);
			IElementHandle elementTypeBotMarker = await curElement.QuerySelectorAsync(_settings.WhatsAppPageExpressions.BotMessageMarker);

			ChatMember senderType = elementTypeBotMarker != null ? ChatMember.Bot : ChatMember.User;

			cancellationToken.ThrowIfCancellationRequested();
			string content = await curElement.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

			string[] tokens = content
				.Split('\n', StringSplitOptions.RemoveEmptyEntries)
				.SkipLast(1) // the last one is time
				.Select(e => e.Trim())
				.ToArray();

			if (tokens.Length == 0)
			{
				continue;
			}

			// ignore prefix link duplication
			if (tokens.Length > 4 && tokens[0] == tokens[2] && tokens[1] == tokens[3])
			{
				tokens = tokens.Skip(2).ToArray();
			}

			messages.Add(new ChatMessage(string.Join('\n', tokens), senderType));
		}

		return messages;
	}

}