using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace FastLane.Chatbot.Browser.Utility;
public static class DebugBrowser
{
	public static string GetHtml(IEnumerable<IElementHandle> elements, ILogger? logger = null, string? filePath = null)
	{
		string result = "";
		foreach (IElementHandle element in elements)
		{
			result += element.GetPropertyAsync("outerHTML").Result.RemoteObject.Value.ToString() + "\n";
		}

		if (result != "")
		{
			if (logger != null)
			{
				logger.LogInformation("{Result}", result);
				return "";
			}
			else if (filePath != null)
			{
				File.WriteAllText(filePath, result);
				return "Wrote to file " + filePath;
			}
		}

		return result;
	}
}
