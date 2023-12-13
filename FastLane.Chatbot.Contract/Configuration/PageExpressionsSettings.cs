namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// Configuration for location of elements needed by chatbot actions
/// </summary>
public record PageExpressionsSettings
{
	public string ContactContainer { get; init; }
	public string SpanChatName { get; init; }
	public string SpanUnreadNumber { get; init; }
}
