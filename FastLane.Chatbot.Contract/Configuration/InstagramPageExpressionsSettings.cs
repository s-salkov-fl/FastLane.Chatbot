namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// Configuration for location of elements needed by chatbot actions
/// </summary>
public record InstagramPageExpressionsSettings
{
	public required string ChatContainer { get; init; }
	public required string ContactContainer { get; init; }
	public required string ContactName { get; init; }
	public required string MessageExistSign { get; init; }
	public required string ChatInput { get; init; }
	public required string BotNickName { get; init; }
	//public required string OpenedChatHeaderNickName { get; init; }
	//public required string ChatMessageContainerExactUserPattern { get; init; }
	//public required string ChatMessageContainerCommon { get; init; }
	//public required string ChatMessageUserName { get; init; }
	//public required string UnreadMessagesCount { get; init; }
}