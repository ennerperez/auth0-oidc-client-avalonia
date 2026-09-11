using System;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityModel.OidcClient.Browser;
using Avalonia.Controls;
using Avalonia.Platform;
using BrowserOptions = Duende.IdentityModel.OidcClient.Browser.BrowserOptions;

namespace Auth0.OidcClient
{
	/// <summary>
	/// Implements the <see cref="IBrowser"/> interface using the <see cref="WebViewBrowser"/> control.
	/// </summary>
	public class WebViewBrowser : IBrowser
	{
		private readonly Func<Window> _windowFactory;
		private readonly bool _shouldCloseWindow;
		private readonly Window _parent;

		/// <summary>
		/// Create a new instance of <see cref="WebViewBrowser"/> with a custom windowFactory and optional window close flag.
		/// </summary>
		/// <param name="windowFactory">A function that returns a <see cref="Window"/> to be used for hosting the browser.</param>
		/// <param name="shouldCloseWindow"> Whether the Window should be closed or not after completion.</param>
		/// <param name="parent">An optional <see cref="Window"/> specifying the parent form.</param>
		public WebViewBrowser(Func<Window> windowFactory, bool shouldCloseWindow, Window parent = null)
		{
			_windowFactory = windowFactory;
			_shouldCloseWindow = shouldCloseWindow;
			_parent = parent;
		}

		/// <summary>
		/// Create a new instance of <see cref="WebViewBrowser"/> with a custom windowFactory and optional window close flag.
		/// </summary>
		/// <param name="windowFactory">A function that returns a <see cref="Window"/> to be used for hosting the browser.</param>
		/// <param name="parent">An optional <see cref="Window"/> specifying the parent form.</param>
		public WebViewBrowser(Func<Window> windowFactory, Window parent = null) : this(windowFactory, true, parent)
		{
		}

		/// <summary>
		/// Create a new instance of <see cref="WebViewBrowser"/> allowing parts of the <see cref="Window"/> container to be set.
		/// </summary>
		/// <param name="title">Optional title for the form - defaults to 'Authenticating...'.</param>
		/// <param name="width">Optional width for the form in pixels. Defaults to 1024.</param>
		/// <param name="height">Optional height for the form in pixels. Defaults to 768.</param>
		/// <param name="minWidth">An optional <see cref="int"/> specifying the min width of the form. Defaults to 1024.</param>
		/// <param name="minHeight">An optional <see cref="int"/> specifying the min height of the form. Defaults to 768.</param>
		/// <param name="maxWidth">An optional <see cref="int"/> specifying the min width of the form. Defaults to 1024.</param>
		/// <param name="maxHeight">An optional <see cref="int"/> specifying the min height of the form. Defaults to 768.</param>
		/// <param name="startupLocation">An optional <see cref="int"/> specifying the startupLocation from parent.</param>
		/// <param name="icon">An optional <see cref="int"/> specifying the form icon.</param>
		/// <param name="parent">An optional <see cref="Window"/> specifying the parent form.</param>
		public WebViewBrowser(string title = "Authenticating...",
			double width = 1024, double height = 768,
			double minWidth = 1024, double minHeight = 768,
			double maxWidth = 1024, double maxHeight = 768,
			WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen, WindowIcon icon = null, Window parent = null)
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
				WindowStartupLocation = startupLocation,
				Icon = icon,
			}, true, parent)
		{
		}

		/// <inheritdoc />
		public Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = new CancellationToken())
		{
			var tcs = new TaskCompletionSource<BrowserResult>();

			var window = _windowFactory();
			var webView = new NativeWebView();
			window.Content = webView;

#if DEBUG
			var gtk = WebViewAdapterInfo.GetAdapterInfo(WebViewAdapterType.WebKitGtk);
			var wpe = WebViewAdapterInfo.GetAdapterInfo(WebViewAdapterType.WpeWebKit);

			Console.WriteLine($"WebKitGTK: {gtk.IsInstalled} {gtk.Version} {gtk.UnavailableReason}");
			Console.WriteLine($"WPE WebKit: {wpe.IsInstalled} {wpe.Version} {wpe.UnavailableReason}");

			webView.EnvironmentRequested += (s, e) =>
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
					// case LinuxWpeWebViewEnvironmentRequestedEventArgs wpeArgs:
					// 	break;
					case GtkWebViewEnvironmentRequestedEventArgs gtkArgs:
						gtkArgs.ForceX11GdkBackend = false;
						gtkArgs.EphemeralDataManager = true;
						break;
				}
			};
#endif

			webView.NavigationStarted += (_, e) =>
			{
				if (e.Request?.ToString()?.StartsWith(options.EndUrl) ?? false)
				{
					tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.Success, Response = e.Request.ToString() });
					if (_shouldCloseWindow)
					{
						if (_parent != null)
						{
							window.Close(tcs);
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
			};

			window.Closing += (_, e) =>
			{
				if (!tcs.Task.IsCompleted)
				{
					tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.UserCancel });
				}

				e.Cancel = true;
				window.Hide();
			};

			if (_parent != null)
			{
				webView.Navigate(new Uri(options.StartUrl));
				window.ShowDialog(_parent);
			}
			else
			{
				window.Show();
				webView.Navigate(new Uri(options.StartUrl));
			}

			return tcs.Task;
		}
	}
}
