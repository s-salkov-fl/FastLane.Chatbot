using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Contract.Actions;

/// <summary>
/// Get last lessages from chat
/// </summary>
public class GetLastMessagesAction : IAction<(IBrowser browser, string chatName, int messagesCount), List<string>>
{
	private readonly Settings _settings;

	public GetLastMessagesAction(IOptionsMonitor<Settings> settings)
	{
		_settings = settings.CurrentValue;
	}

	public async Task<List<string>> InvokeActionAsync((IBrowser browser, string chatName, int messagesCount) argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		(IBrowser browser, string chatName, int messagesCount) = argument;

		if (messagesCount == -1)
		{ messagesCount = int.MaxValue; }

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.PageExpressions.UserCommentBoxXpath);

		List<string> messages = new();
		for (int i = elements.Length; i > 0; i--)
		{
			cancellationToken.ThrowIfCancellationRequested();
			string content = await elements[i - 1].EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

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

			messages.Add(string.Join('\n', tokens));
			if (messages.Count >= messagesCount)
			{ break; }
		}

		return messages;
	}

}
