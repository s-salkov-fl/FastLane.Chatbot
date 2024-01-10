using System.Collections.Concurrent;
using FastLane.Chatbot.TikTok.Services;

namespace FastLane.Chatbot.TikTok.Model;
public class TikTokClientsPool : ConcurrentBag<ITikTokClient>
{
}