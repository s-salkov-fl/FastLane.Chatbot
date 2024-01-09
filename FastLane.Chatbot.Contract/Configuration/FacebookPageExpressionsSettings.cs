namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// Configuration for location of elements needed by chatbot actions
/// </summary>
public record FacebookPageExpressionsSettings
{
	public required string ChatContainer { get; init; }
	public required string ContactContainer { get; init; }
	public required string ChatIdLink { get; init; }
	public required string ChatId { get; init; }
	public required string ContactName { get; init; }
	public required string MessageExistSign { get; init; }
	public required string ChatMessageContainer { get; init; }
	public required string ChatInput { get; init; }
}