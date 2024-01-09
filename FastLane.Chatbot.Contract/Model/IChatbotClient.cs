namespace FastLane.Chatbot.Contract.Model;
public interface IChatbotClient : IAsyncDisposable
{
	/// <summary>
	/// New message was received
	/// </summary>
	event Action? MessageReceived;

	/// <summary>
	/// Read last available messages from chat with given name(user name)
	/// </summary>
	Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string chat, CancellationToken cancellationToken);

	/// <summary>
	/// Get list of chats with number of unread messages inside. (Calls <see cref="MessageReceived"/> event if statistics was changed since last query).
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IReadOnlyDictionary<string, int>> GetChatInboxCountAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Open chat with specified name, send message and close chat
	/// </summary>
	Task PostAsync(string chat, string content, CancellationToken cancellationToken);
}
