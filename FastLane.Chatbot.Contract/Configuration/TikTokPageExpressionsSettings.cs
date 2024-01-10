namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// Configuration for location of elements needed by chatbot actions
/// </summary>
public record TikTokPageExpressionsSettings
{
	public required string ChatContainer { get; init; }
	public required string ContactContainer { get; init; }
	public required string ChatMessageContainerPattern { get; init; }
	public required string ChatInput { get; init; }
}