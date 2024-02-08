using FastLane.Chatbot.Application.Services;
using FastLane.Chatbot.Browser;
using FastLane.Chatbot.WhatsApp;
using FastLane.Chatbot.Facebook;
using FastLane.Chatbot.TikTok;
using FastLane.Chatbot.Instagram;
using FastLane.Chatbot.TwitterX;
using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using FastLane.Chatbot.WhatsAppNodeJsClient;

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((context, services) =>
{
	services
	.AddOptions()
		.Configure<Settings>(context.Configuration)
		.AddSingleton(e => e.GetRequiredService<IOptionsMonitor<Settings>>().CurrentValue.Browser)
		.AddBrowser()
		.AddWhatsApp()
		.AddWhatsAppChatbot()
		.AddFacebook()
		.AddFacebookChatbot()
		.AddTikTok()
		.AddTikTokChatbot()
		.AddInstagram()
		.AddInstagramChatbot()
		.AddTwitterX()
		.AddTwitterXChatbot()
		.AddWhatsAppNodeJs()
		.AddWhatsAppNodeJsChatbot()
		;

	services.AddHostedService<AppService>();
})
.Build();

await host.RunAsync();
