using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace FastLane.Chatbot.Contract.Actions;

/// <summary>
/// Finds chat name in chat list and clicks on it, i.e. enters exact chat
/// </summary>
public class EnterChatAction : IAction<(IBrowser browser, string chatName), bool>
{
	private readonly Settings _settings;

	public EnterChatAction(IOptionsMonitor<Settings> settings)
	{
		_settings = settings.CurrentValue;
	}

	public async Task<bool> InvokeActionAsync((IBrowser browser, string chatName) argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		(IBrowser browser, string chatName) = argument;

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		TimeSpan timeout = TimeSpan.FromMilliseconds(_settings.WhatsApp.TimeOutContactsRefreshMs);
		DateTime limit = DateTime.Now + timeout;
		List<IElementHandle> chatNamesElements = new();

		while (!cancellationToken.IsCancellationRequested && limit > DateTime.Now && chatNamesElements.Count == 0)
		{
			try
			{
				chatNamesElements.AddRange(await page.QuerySelectorAllAsync(_settings.PageExpressions.ContactContainer));
			}
			catch
			{// Skip
			}

			if (chatNamesElements.Count == 0)
			{
				await Task.Delay(1000, cancellationToken);
				continue;
			}
		}

		foreach (IElementHandle element in chatNamesElements)
		{
			IElementHandle spanChatName = await element.QuerySelectorAsync(_settings.PageExpressions.SpanChatName);
			string curChatName = await spanChatName.EvaluateFunctionAsync<string>("(e) => { return e.innerText; }");

			if (curChatName == chatName)
			{
				await element.ClickAsync();
				await Task.Delay(_settings.WhatsApp.GeneralMutateFailCrutchWaitMs, cancellationToken);
				return true;
			}
		}

		return false;
	}

}
