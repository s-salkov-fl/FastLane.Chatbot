namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// Whatsapp nodeJs chat bot constants
/// </summary>
public record WhatsAppNodeJsChatBotSettings
{
	public required string NodeJsApplicationPath { get; init; }
	public required string NodeJsExecModulePath { get; init; }
	public required int AuthQrPollIntervalMs { get; init; }
	public required int NewStateEventPollingPeriodMs { get; init; }
	public required int IsReadyClientFailPeriodMs { get; init; }
}