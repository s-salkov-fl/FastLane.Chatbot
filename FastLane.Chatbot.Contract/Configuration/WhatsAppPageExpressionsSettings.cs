namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// Configuration for location of elements needed by chatbot actions
/// </summary>
public record WhatsAppPageExpressionsSettings
{
	public required string ContactContainer { get; init; }
	public required string SpanChatName { get; init; }
	public required string SpanUnreadNumber { get; init; }
	public required string SearchInputXpath { get; init; }
	public required string UserCommentBoxXpath { get; init; }
	public required string BotCommentBoxXpath { get; init; }
	public required string UserAndBotCommentBoxXpath { get; init; }
	public required string UserMessageMarker { get; init; }
	public required string BotMessageMarker { get; init; }
	public required string ChatInputXpath { get; init; }
}