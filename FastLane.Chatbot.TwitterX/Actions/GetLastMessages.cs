using FastLane.Chatbot.Contract.Configuration;
using FastLane.Chatbot.Contract.Model;
using FastLane.Chatbot.Contract.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;

namespace FastLane.Chatbot.TwitterX.Actions;

/// <summary>
/// Get last lessages from active opened chat
/// </summary>
public partial class GetLastMessages(IOptionsMonitor<Settings> settings)
{
	private readonly Settings _settings = settings.CurrentValue;

	private const string _messagesGetScript = @"
function getListOfElementsByXPath(xpath) {
    var result = document.evaluate(xpath, document, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);
    return result;
}

function getMessages() {
var clearMessCont = getListOfElementsByXPath(`//section[@aria-label='Section details' and @role='region']//div[@data-testid='cellInnerDiv']/../div`).iterateNext();

var clearMessRect = clearMessCont.getBoundingClientRect();
var contLeftX = clearMessRect.left;
var contRightX = clearMessRect.right;
var result = [];

var messCont = getListOfElementsByXPath(`//section[@aria-label='Section details' and @role='region']//div[@data-testid='cellInnerDiv']//div[@data-testid='messageEntry']//div[@role='presentation']`);

var currentMessageElement;

while (currentMessageElement = messCont.iterateNext())
{
 var currentRect = currentMessageElement.getBoundingClientRect();
 var currentMessage = (currentMessageElement.innerText) ? currentMessageElement.innerText : '';

 result.push([ (currentRect.left == contLeftX) ? 2 : 1, currentMessage]);
}

return result;
}

try { getMessages(); } catch(e) { throw new Error(`Error executing javaScript for getting messages for chat: ` + e); }";

	public async Task<IReadOnlyList<ChatMessage>> InvokeActionAsync(IBrowser browser, string botNickName, ILogger logger, ChatMember memberFilter, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		botNickName = botNickName.NormalizeSpaces();

		IPage[] pages = await browser.PagesAsync();
		IPage page = pages.FirstOrDefault() ?? await browser.NewPageAsync();

		//File.WriteAllText("C:\\temp\\page.txt", await page.GetContentAsync());

		await page.WaitForSelectorAsync(_settings.TwitterXPageExpressions.ChatMessagesContainer);

		JToken result = await page.EvaluateExpressionAsync(_messagesGetScript);

		List<ChatMessage> messages = [];

		int messNum = 0;

		foreach (JToken curResult in result)
		{
			messNum++;
			int? messageType = ((curResult?.First as IConvertible)?.ToInt32(null))
				?? throw new InvalidOperationException("Invalid message type from javascript,- equals: " + (curResult?.First?.ToString() ?? "NULL"));

			string message = (curResult?.Last)?.ToString() ?? "";
			messages.Add(new ChatMessage(message, (ChatMember)messageType));
		}

		return messages;
	}
}