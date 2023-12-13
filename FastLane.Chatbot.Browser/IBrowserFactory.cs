using FastLane.Chatbot.Browser.Configuration;
using PuppeteerSharp;

namespace FastLane.Chatbot.Browser;

/// <summary>
/// Browser launcher for Puppeteer
/// </summary>
public interface IBrowserFactory
{
	Task<IBrowser> CreateBrowserAsync(string profile, CancellationToken cancellationToken);
}

internal class BrowserFactory : IBrowserFactory, IDisposable
{
	private static readonly IReadOnlyList<string> _browserArgs = new[]
	{
		"--no-sandbox", " --disable-setuid-sandbox", "--disable-dev-shm-usage", "--disable-software-rasterizer",
		"--disable-gpu", "--disable-blink-features=AutomationControlled", "--disable-background-networking",
		"--enable-features=NetworkService,NetworkServiceInProcess", "--disable-background-timer-throttling",
		"--disable-backgrounding-occluded-windows", "--disable-breakpad",
		"--disable-client-side-phishing-detection", "--disable-component-extensions-with-background-pages",
		"--disable-default-apps", "--disable-extensions", "--disable-features=TranslateUI",
		"--disable-hang-monitor", "--disable-ipc-flooding-protection", "--disable-popup-blocking",
		"--disable-prompt-on-repost", "--disable-renderer-backgrounding", "--disable-sync",
		"--force-color-profile=srgb", "--metrics-recording-only", "--no-first-run", "--password-store=basic",
		"--use-mock-keychain", "--disable-features=IsolateOrigins,site-per-process", "--use-gl=swiftshader",
	};

	private readonly SemaphoreSlim _sync = new(1, 1);
	private readonly BrowserSettings _settings;


	public BrowserFactory(BrowserSettings settings)
	{
		_settings = settings;
	}


	public async Task<IBrowser> CreateBrowserAsync(string profile, CancellationToken cancellationToken)
	{
		await _sync.WaitAsync(cancellationToken);

		try
		{
			return await Puppeteer.LaunchAsync(options: new LaunchOptions
			{
				Headless = false,
				ExecutablePath = _settings.Path,
				UserDataDir = CreateProfilePath(profile),
				Args = _browserArgs.ToArray(),
				DefaultViewport = null
			});
		}
		finally
		{
			_sync.Release();
		}
	}

	public void Dispose()
	{
		_sync.Dispose();
	}

	/// <summary>
	/// Create folder for store browser config profile
	/// </summary>
	/// <param name="profile">Name of folder to store browser's profile configuration to store between application launches</param>
	/// <returns>Path to folder created</returns>
	private string CreateProfilePath(string profile)
	{
		string path = GetType().Assembly.Location;
		path = string.IsNullOrWhiteSpace(path)
			? Environment.CurrentDirectory
			: new FileInfo(path).DirectoryName ?? Environment.CurrentDirectory;

		path = Path.Combine(path, "profiles");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		path = Path.Combine(path, profile);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		return path;
	}
}