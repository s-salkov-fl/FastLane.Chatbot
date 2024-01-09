using FastLane.Chatbot.Application.Services;
using FastLane.Chatbot.Browser;
using FastLane.Chatbot.WhatsApp;
using FastLane.Chatbot.Facebook;
using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;

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
		.AddFacebookChatbot();

	services.AddHostedService<AppService>();
})
.Build();

await host.RunAsync();
