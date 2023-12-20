using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace FastLane.Chatbot.Contract.Actions;

/// <summary>
/// Type message text into dialog textbox and press Enter key to send
/// </summary>
public class PostMessage
{
	private readonly Settings _settings;

	public PostMessage(IOptionsMonitor<Settings> settings)
	{
		_settings = settings.CurrentValue;
	}

	public async Task<bool> InvokeActionAsync((IBrowser browser, string messageText) argument, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		(IBrowser browser, string messageText) = argument;

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		IElementHandle input = await page.WaitForSelectorAsync(_settings.PageExpressions.ChatInputXpath);

		await input.TypeAsync(messageText, new TypeOptions { Delay = 10 });

		await page.Keyboard.PressAsync(Key.Enter);
		await Task.Delay(_settings.WhatsApp.GeneralMutateFailCrutchWaitMs, cancellationToken);

		return false;
	}

}
