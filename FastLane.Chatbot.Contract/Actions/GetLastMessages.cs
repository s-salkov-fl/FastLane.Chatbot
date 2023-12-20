using System.Xml.Linq;
using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Contract.Actions;

/// <summary>
/// Get last lessages from chat
/// </summary>
public class GetLastMessages
{
	private readonly Settings _settings;

	public GetLastMessages(IOptionsMonitor<Settings> settings)
	{
		_settings = settings.CurrentValue;
	}

	public async Task<IReadOnlyList<ChatMessage>> InvokeActionAsync(IBrowser browser, ChatMember memberFilter, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		string neededFilter = memberFilter.HasFlag(ChatMember.User | ChatMember.Bot) ? _settings.PageExpressions.UserAndBotCommentBoxXpath
			: memberFilter.HasFlag(ChatMember.Bot) ? _settings.PageExpressions.BotCommentBoxXpath : _settings.PageExpressions.UserCommentBoxXpath;

		IElementHandle[] elements = await page.QuerySelectorAllAsync(neededFilter);

		List<ChatMessage> messages = new();
		for (int i = elements.Length; i > 0; i--)
		{
			IElementHandle curElement = elements[i - 1];

			//IElementHandle elementTypeUserMarker = await curElement.QuerySelectorAsync(_settings.PageExpressions.UserMessageMarker);
			IElementHandle elementTypeBotMarker = await curElement.QuerySelectorAsync(_settings.PageExpressions.BotMessageMarker);

			ChatMember senderType = (elementTypeBotMarker != null) ? ChatMember.Bot : ChatMember.User;

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
