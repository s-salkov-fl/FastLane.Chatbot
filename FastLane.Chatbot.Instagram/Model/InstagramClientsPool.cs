using System.Collections.Concurrent;
using FastLane.Chatbot.Instagram.Services;

namespace FastLane.Chatbot.Instagram.Model;
public class InstagramClientsPool : ConcurrentBag<IInstagramClient>
{
}