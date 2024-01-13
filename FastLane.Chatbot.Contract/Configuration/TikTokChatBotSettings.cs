namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// General tiktok chat bot constants
/// </summary>
public record TikTokChatBotSettings
{
	public required int NewStateEventPollingPeriodMs { get; init; }
	public required int GeneralMutateFailCrutchWaitMs { get; init; }
}