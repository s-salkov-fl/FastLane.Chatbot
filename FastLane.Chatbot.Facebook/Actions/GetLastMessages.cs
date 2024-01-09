using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Facebook.Actions;

/// <summary>
/// Get last lessages from chat
/// </summary>
public class GetLastMessages(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<IReadOnlyList<ChatMessage>> InvokeActionAsync(IBrowser browser, ChatMember memberFilter, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		string CommentBoxXpath = _settings.FaceBookPageExpressions.ChatMessageContainer;

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		IElementHandle[] elements = await page.QuerySelectorAllAsync(CommentBoxXpath);

		List<ChatMessage> messages = [];
		for (int i = elements.Length; i > 0; i--)
		{
			cancellationToken.ThrowIfCancellationRequested();
			string content = await elements[i - 1].EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

			string[] tokens = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
			if (tokens.Length < 2)
			{
				continue;
			}

			string userName = tokens[0];
			string messageBody = string.Join('\n', tokens.Skip(1).Take(tokens.Length - 2));

			if (userName.Equals("You sent", StringComparison.OrdinalIgnoreCase) && memberFilter.HasFlag(ChatMember.Bot))
			{
				messages.Add(new ChatMessage(messageBody, ChatMember.Bot));
			}
			else if (memberFilter.HasFlag(ChatMember.User))
			{
				messages.Add(new ChatMessage(messageBody, ChatMember.User));
			}
		}

		return messages;
	}

}