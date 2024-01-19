using System.Collections.Concurrent;
using FastLane.Chatbot.TwitterX.Services;

namespace FastLane.Chatbot.TwitterX.Model;
public class TwitterXClientsPool : ConcurrentBag<ITwitterXClient>
{
}