using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;

namespace FastLane.Chatbot.Contract.Actions;

/// <summary>
/// Closes currently opened chat
/// </summary>
public class CloseChatAction : IAction<IBrowser, bool>
{
	private readonly Settings _settings;

	public CloseChatAction(IOptionsMonitor<Settings> settings)
	{
		_settings = settings.CurrentValue;
	}

	public async Task<bool> InvokeActionAsync(IBrowser argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (argument == null)
		{ throw new ArgumentNullException(nameof(argument)); }

		IBrowser browser = argument;

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		await page.Keyboard.PressAsync("Escape");
		await Task.Delay(_settings.WhatsApp.GeneralMutateFailCrutchWaitMs, cancellationToken);

		return true;
	}

}
