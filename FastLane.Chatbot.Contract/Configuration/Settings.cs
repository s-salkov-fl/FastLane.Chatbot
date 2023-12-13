using FastLane.Chatbot.Browser.Configuration;

namespace FastLane.Chatbot.Contract.Configuration;

public record Settings
{
	public BrowserSettings Browser { get; init; } = default!;
	public PageExpressionsSettings PageExpressions { get; init; }
}