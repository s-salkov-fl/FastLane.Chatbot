namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// General facebook chat bot constants
/// </summary>
public record FacebookChatBotSettings
{
	public int NewStateEventPollingPeriodMs { get; init; }
	public int GeneralMutateFailCrutchWaitMs { get; init; }
}