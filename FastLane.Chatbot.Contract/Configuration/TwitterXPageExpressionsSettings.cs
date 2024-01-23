namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// Configuration for location of elements needed by chatbot actions
/// </summary>
public record TwitterXPageExpressionsSettings
{
	public required string ChatContainer { get; init; }
	public required string ContactContainer { get; init; }
	public required string ContactName { get; init; }
	public required string MessageExistSign { get; init; }
	public required string ChatMessagesContainer { get; init; }
	public required string ChatInput { get; init; }
	public required string BotNickName { get; init; }
	public required string ChatMessageText { get; init; }
}