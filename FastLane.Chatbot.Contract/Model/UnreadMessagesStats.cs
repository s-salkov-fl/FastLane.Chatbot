namespace FastLane.Chatbot.Contract.Model;

/// <summary>
/// Stores statistics about unread messages
/// </summary>
public class UnreadMessagesStats
{
	/// <summary>
	/// Chat names and number of unread messages
	/// </summary>
	public Dictionary<string, int> Messages { get; set; } = [];

	/// <summary>
	/// Checks whether chat names and number unread messages are matching in two instances
	/// </summary>
	public override bool Equals(object? obj)
	{
		if (obj is UnreadMessagesStats unreadMessagesStats)
		{
			if (Messages.Count != unreadMessagesStats.Messages.Count)
			{ return false; }

			foreach (KeyValuePair<string, int> message in Messages)
			{
				if (!unreadMessagesStats.Messages.TryGetValue(message.Key, out int value))
				{ return false; }

				if (value != message.Value)
				{ return false; }
			}

			return true;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return Messages.GetHashCode();
	}
}