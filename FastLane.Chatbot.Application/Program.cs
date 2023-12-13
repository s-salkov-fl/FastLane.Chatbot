using FastLane.Chatbot.Application.Services;
using FastLane.Chatbot.Contract.Configuration;
using Microsoft.Extensions.Options;
using FastLane.Chatbot.Browser;
using FastLane.Chatbot.Contract;

IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((context, services) =>
{
	services
	.AddOptions()
		.Configure<Settings>(context.Configuration)
		.AddSingleton(e => e.GetRequiredService<IOptionsMonitor<Settings>>().CurrentValue.Browser)
		.AddBrowser()
		.AddWhatsAppChatbot();

	services.AddHostedService<AppService>();
})
.Build();

await host.RunAsync();
