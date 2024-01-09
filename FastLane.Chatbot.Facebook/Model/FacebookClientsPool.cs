using System.Collections.Concurrent;
using FastLane.Chatbot.Facebook.Services;

namespace FastLane.Chatbot.Facebook.Model;
public class FacebookClientsPool : ConcurrentBag<IFacebookClient>
{
}