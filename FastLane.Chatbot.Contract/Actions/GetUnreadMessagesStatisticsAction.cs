using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Contract.Actions;

/// <summary>
/// Action for collect unread messages statistics
/// </summary>
public class GetUnreadMessagesStatisticsAction : IAction<IBrowser, UnreadMessagesStats>
{
	private readonly Settings _settings;
	public GetUnreadMessagesStatisticsAction(IOptionsMonitor<Settings> settings)
	{
		_settings = settings.CurrentValue;
	}

	public async Task<UnreadMessagesStats> InvokeActionAsync(IBrowser argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (argument == null)
		{ throw new ArgumentNullException(nameof(argument)); }
		IBrowser _browser = argument;

		IPage[] pages = await _browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await _browser.NewPageAsync();

		IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.PageExpressions.ContactContainer);
		UnreadMessagesStats result = new();

		foreach (IElementHandle element in elements)
		{
			IElementHandle spanChatName = await element.QuerySelectorAsync(_settings.PageExpressions.SpanChatName);
			string chatName = await spanChatName.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

			IElementHandle spanUnreadNumber = await element.QuerySelectorAsync(_settings.PageExpressions.SpanUnreadNumber);

			if (spanUnreadNumber != null)
			{
				string theNumber = await spanUnreadNumber.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

				if (theNumber != null)
				{ result.Messages[chatName] = int.Parse(theNumber); }
			}
		}

		return result;
	}

}
