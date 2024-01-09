using System.Text.RegularExpressions;
using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.WhatsApp.Actions;

/// <summary>
/// Action for collect unread messages statistics
/// </summary>
public partial class GetUnreadMessagesStatistics(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<IReadOnlyDictionary<string, int>> InvokeActionAsync(IBrowser argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (argument == null)
		{ throw new ArgumentNullException(nameof(argument)); }
		IBrowser _browser = argument;

		IPage[] pages = await _browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await _browser.NewPageAsync();

		IElementHandle[] elements = await page.QuerySelectorAllAsync(_settings.WhatsAppPageExpressions.ContactContainer);
		Dictionary<string, int> result = [];

		foreach (IElementHandle element in elements)
		{
			IElementHandle spanChatName = await element.QuerySelectorAsync(_settings.WhatsAppPageExpressions.SpanChatName);
			string chatName = await spanChatName.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

			IElementHandle spanUnreadNumber = await element.QuerySelectorAsync(_settings.WhatsAppPageExpressions.SpanUnreadNumber);

			if (spanUnreadNumber != null)
			{
				string theNumber = await spanUnreadNumber.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

				if (theNumber != null)
				{
					theNumber = NumRegex().Replace(theNumber, "");
					if (!string.IsNullOrWhiteSpace(theNumber))
					{ result[chatName] = int.Parse(theNumber); }
				}
			}
		}

		return result;
	}

	[GeneratedRegex("[^0-9]")]
	private static partial Regex NumRegex();
}