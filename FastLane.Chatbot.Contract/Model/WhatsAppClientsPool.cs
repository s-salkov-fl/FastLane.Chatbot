using System.Collections.Concurrent;
using FastLane.Chatbot.Contract.Services;

namespace FastLane.Chatbot.Contract.Model;
internal class WhatsAppClientsPool : ConcurrentBag<IWhatsAppClient>
{
}
