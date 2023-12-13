namespace FastLane.Chatbot.Browser.Configuration;

/// <summary>
/// Configuration for launching browser by Puppeteer
/// </summary>
public record BrowserSettings
{
	public string Path { get; init; }
	public string ProfileConfigName { get; init; }
}