namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// General tiktok chat bot constants
/// </summary>
public record TikTokChatBotSettings
{
	public int NewStateEventPollingPeriodMs { get; init; }
}