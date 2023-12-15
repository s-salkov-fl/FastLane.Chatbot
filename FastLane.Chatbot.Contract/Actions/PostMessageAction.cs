using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace FastLane.Chatbot.Contract.Actions;

/// <summary>
/// Type message text into dialog textbox and press Enter key to send
/// </summary>
public class PostMessageAction : IAction<(IBrowser browser, string messageText), bool>
{
	private readonly Settings _settings;

	public PostMessageAction(IOptionsMonitor<Settings> settings)
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
		await Task.Delay(2000, cancellationToken);

		return false;
	}

}
