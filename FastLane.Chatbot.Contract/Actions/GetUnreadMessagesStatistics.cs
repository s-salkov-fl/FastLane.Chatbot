using System.Text.RegularExpressions;
using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Contract.Actions;

/// <summary>
/// Action for collect unread messages statistics
/// </summary>
public class GetUnreadMessagesStatistics
{
	private readonly Settings _settings;
	public GetUnreadMessagesStatistics(IOptionsMonitor<Settings> settings)
	{
		_settings = settings.CurrentValue;
	}

	public async Task<IReadOnlyDictionary<string, int>> InvokeActionAsync(IBrowser argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (argument == null)
		{ throw new ArgumentNullException(nameof(argument)); }
		IBrowser _browser = argument;

		IPage[] pages = await _browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await _browser.NewPageAsync();

		IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.PageExpressions.ContactContainer);
		Dictionary<string, int> result = new();

		foreach (IElementHandle element in elements)
		{
			IElementHandle spanChatName = await element.QuerySelectorAsync(_settings.PageExpressions.SpanChatName);
			string chatName = await spanChatName.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

			IElementHandle spanUnreadNumber = await element.QuerySelectorAsync(_settings.PageExpressions.SpanUnreadNumber);

			if (spanUnreadNumber != null)
			{
				string theNumber = await spanUnreadNumber.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

				if (theNumber != null)
				{
					theNumber = Regex.Replace(theNumber, "[^0-9]", "");
					if (!string.IsNullOrWhiteSpace(theNumber))
					{ result[chatName] = int.Parse(theNumber); }
				}
			}
		}

		return result;
	}

}
