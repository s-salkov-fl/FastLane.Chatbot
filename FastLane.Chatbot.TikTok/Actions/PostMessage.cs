using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace FastLane.Chatbot.TikTok.Actions;

/// <summary>
/// Type message text into dialog textbox and press Enter key to send
/// </summary>
public class PostMessage(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	public async Task<bool> InvokeActionAsync(IBrowser browser, string messageText, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		IElementHandle input = await page.WaitForSelectorAsync(_settings.TikTokPageExpressions.ChatInput);

		await input.ClickAsync();
		await Task.Delay(500, cancellationToken);
		await input.TypeAsync(messageText, new TypeOptions { Delay = 10 });

		await page.Keyboard.PressAsync(Key.Enter);

		return true;
	}

}