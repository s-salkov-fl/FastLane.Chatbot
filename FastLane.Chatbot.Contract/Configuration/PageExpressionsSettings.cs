namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// Configuration for location of elements needed by chatbot actions
/// </summary>
public record PageExpressionsSettings
{
	public string ContactContainer { get; init; }
	public string SpanChatName { get; init; }
	public string SpanUnreadNumber { get; init; }
	public string SearchInputXpath { get; init; }
	public string UserCommentBoxXpath { get; init; }
	public string BotCommentBoxXpath { get; init; }
	public string UserAndBotCommentBoxXpath { get; init; }
	public string UserMessageMarker { get; init; }
	public string BotMessageMarker { get; init; }
	public string ChatInputXpath { get; init; }
}
