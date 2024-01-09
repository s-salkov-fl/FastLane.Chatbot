using System.Collections.Concurrent;
using FastLane.Chatbot.WhatsApp.Services;

namespace FastLane.Chatbot.WhatsApp.Model;
internal class WhatsAppClientsPool : ConcurrentBag<IWhatsAppClient>
{
}