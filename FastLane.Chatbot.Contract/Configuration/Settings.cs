namespace FastLane.Chatbot.Contract.Configuration;

public record Settings
{
	public BrowserSettings Browser { get; init; } = default!;
	public required WhatsAppPageExpressionsSettings WhatsAppPageExpressions { get; init; }
	public required FacebookPageExpressionsSettings FaceBookPageExpressions { get; init; }
	public required TikTokPageExpressionsSettings TikTokPageExpressions { get; init; }
	public required WhatsAppChatBotSettings WhatsApp { get; init; }
	public required FacebookChatBotSettings Facebook { get; init; }
	public required TikTokChatBotSettings TikTok { get; init; }
}