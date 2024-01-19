namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// General TwitterX chat bot constants
/// </summary>
public record TwitterXChatBotSettings
{
	public required int NewStateEventPollingPeriodMs { get; init; }
	public required int GeneralMutateFailCrutchWaitMs { get; init; }
}