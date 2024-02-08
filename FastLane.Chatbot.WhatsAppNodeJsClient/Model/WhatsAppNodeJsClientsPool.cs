using System.Collections.Concurrent;
using FastLane.Chatbot.WhatsAppNodeJsClient.Services;

namespace FastLane.Chatbot.WhatsAppNodeJsClient.Model;
public class WhatsAppNodeJsClientsPool : ConcurrentBag<IWhatsAppNodeJsClient>
{
}