namespace FastLane.Chatbot.Contract.Configuration;

/// <summary>
/// General whatsapp chat bot constants
/// </summary>
public record WhatsAppChatBotSettings
{
	public int TimeOutContactsRefreshMs { get; init; }
	public int GeneralMutateFailCrutchWaitMs { get; init; }
}
