namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// General Instagram chat bot constants
/// </summary>
public record InstagramChatBotSettings
{
	public required int NewStateEventPollingPeriodMs { get; init; }
	public required int GeneralMutateFailCrutchWaitMs { get; init; }
}