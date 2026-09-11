using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Duende.IdentityModel.OidcClient.Browser;
using BrowserOptions = Duende.IdentityModel.OidcClient.Browser.BrowserOptions;

namespace Auth0.OidcClient
{
	/// <summary>
	/// Implements the <see cref="IBrowser"/> interface using <see cref="WebBrowserBrowser"/>.
	/// </summary>
	public class WebBrowserBrowser : IBrowser
	{
		private readonly Func<Window> _windowFactory;
		private readonly bool _shouldCloseWindow;
		private readonly Window _parent;

		/// <summary>
		/// Create a new instance of <see cref="WebBrowserBrowser"/> with a custom Window factory and optional flag to indicate if the window should be closed.
		/// </summary>
		/// <param name="windowFactory">A function that returns a <see cref="Window"/> to be used for hosting the browser.</param>
		/// <param name="shouldCloseWindow"> Whether the Window should be closed or not after completion.</param>
		/// <param name="parent">An optional <see cref="Window"/> specifying the parent form.</param>
		public WebBrowserBrowser(Func<Window> windowFactory, bool shouldCloseWindow = true, Window parent = null)
		{
			_windowFactory = windowFactory;
			_shouldCloseWindow = shouldCloseWindow;
			_parent = parent;
		}


		/// <summary>
		/// Create a new instance of <see cref="WebBrowserBrowser"/> with a custom Window factory and optional flag to indicate if the window should be closed.
		/// </summary>
		/// <param name="windowFactory">A function that returns a <see cref="Window"/> to be used for hosting the browser.</param>
		/// <param name="parent">An optional <see cref="Window"/> specifying the parent form.</param>
		public WebBrowserBrowser(Func<Window> windowFactory, Window parent = null) : this(windowFactory, true, parent)
		{
		}

		/// <summary>
		/// Create a new instance of <see cref="WebBrowserBrowser"/> that will create a customized <see cref="Window"/> as needed.
		/// </summary>
		/// <param name="title">An optional <see cref="string"/> specifying the title of the form. Defaults to "Authenticating...".</param>
		/// <param name="width">An optional <see cref="int"/> specifying the width of the form. Defaults to 1024.</param>
		/// <param name="height">An optional <see cref="int"/> specifying the height of the form. Defaults to 768.</param>
		/// <param name="minWidth">An optional <see cref="int"/> specifying the min width of the form. Defaults to 1024.</param>
		/// <param name="minHeight">An optional <see cref="int"/> specifying the min height of the form. Defaults to 768.</param>
		/// <param name="maxWidth">An optional <see cref="int"/> specifying the min width of the form. Defaults to 1024.</param>
		/// <param name="maxHeight">An optional <see cref="int"/> specifying the min height of the form. Defaults to 768.</param>
		/// <param name="windowStartupLocation">An optional <see cref="int"/> specifying the startupLocation from parent.</param>
		/// <param name="icon">An optional <see cref="int"/> specifying the form icon.</param>
		/// <param name="windowDecorations">An optional <see cref="WindowDecorations"/> specifying the form system decorations.</param>
		/// <param name="canResize">An optional <see cref="bool"/> specifying the can resize option.</param>
		/// <param name="parent">An optional <see cref="Window"/> specifying the parent form.</param>
		public WebBrowserBrowser(string title = "Authenticating...",
			double width = 1024, double height = 768,
			double minWidth = 1024, double minHeight = 768,
			double maxWidth = 1024, double maxHeight = 768,
			WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterScreen,
			WindowIcon icon = null,
			WindowDecorations windowDecorations = WindowDecorations.Full,
			bool canResize = true,
			Window parent = null)
			: this(() => new Window
			{
				Name = "WebAuthentication",
				Title = title,
				Width = width,
				Height = height,
				MinHeight = minHeight,
				MinWidth = minWidth,
				MaxHeight = maxHeight,
				MaxWidth = maxWidth,
				WindowStartupLocation = windowStartupLocation,
				Icon = icon ?? parent?.Icon,
				WindowDecorations = windowDecorations,
				CanResize = canResize
			}, parent)
		{
		}

		/// <inheritdoc />
		public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = new CancellationToken())
		{
			var window = _windowFactory.Invoke();

			var browser = new NativeWebView();
#if DEBUG
			var gtk = WebViewAdapterInfo.GetAdapterInfo(WebViewAdapterType.WebKitGtk);
			var wpe = WebViewAdapterInfo.GetAdapterInfo(WebViewAdapterType.WpeWebKit);

			Console.WriteLine($"WebKitGTK: {gtk.IsInstalled} {gtk.Version} {gtk.UnavailableReason}");
			Console.WriteLine($"WPE WebKit: {wpe.IsInstalled} {wpe.Version} {wpe.UnavailableReason}");

			browser.EnvironmentRequested += (s, e) =>
			{
				// Enable developer tools for all platforms
				e.EnableDevTools = true;

				// Platform-specific configuration
				switch (e)
				{
					case WindowsWebView2EnvironmentRequestedEventArgs webView2Args:
						webView2Args.IsInPrivateModeEnabled = true;
						break;
					case AppleWKWebViewEnvironmentRequestedEventArgs appleArgs:
						appleArgs.NonPersistentDataStore = true;
						break;
					case GtkWebViewEnvironmentRequestedEventArgs gtkArgs:
						gtkArgs.EphemeralDataManager = true;
						break;
				}
			};
#endif
			var signal = new SemaphoreSlim(0, 1);
			var result = new BrowserResult
			{
				ResultType = BrowserResultType.UserCancel
			};

			window.Closed += (_, _) => { signal.Release(); };
			window.Closing += (_, e) =>
			{
				e.Cancel = true;
				window.Hide();
			};
			browser.NavigationCompleted += (_, e) =>
			{
				if (e.Request?.ToString().StartsWith(options.EndUrl) ?? false)
				{
					result.ResultType = BrowserResultType.Success;
					result.Response = e.Request?.ToString();
					signal.Release();
				}
			};
			
			try
			{
				window.Content = browser;

				if (_parent != null)
				{
					browser.Navigate(new Uri(options.StartUrl));
					await window.ShowDialog(_parent);
				}
				else
				{
					window.Show();
					browser.Navigate(new Uri(options.StartUrl));
				}

				await signal.WaitAsync(cancellationToken);
			}
			finally
			{
				if (_shouldCloseWindow)
				{
					if (_parent != null)
					{
						window.Close(result);
					}
					else
					{
						window.Close();
					}
				}
				else
				{
					window.Content = null;
				}
			}

			return result;
		}
	}
}
