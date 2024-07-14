using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaWebView;
using IdentityModel.OidcClient.Browser;

namespace Auth0.OidcClient
{
	/// <summary>
	/// Implements the <see cref="IBrowser"/> interface using <see cref="WebBrowser"/>.
	/// </summary>
	public class WebBrowser : IBrowser
	{
		private readonly Func<Window> _windowFactory;
		private readonly bool _shouldCloseWindow;
		private readonly Window _parent;

		/// <summary>
		/// Create a new instance of <see cref="WebBrowser"/> with a custom Window factory and optional flag to indicate if the window should be closed.
		/// </summary>
		/// <param name="windowFactory">A function that returns a <see cref="Window"/> to be used for hosting the browser.</param>
		/// <param name="shouldCloseWindow"> Whether the Window should be closed or not after completion.</param>
		/// <example>
		/// This sample shows how to call the <see cref="WebBrowser(Func&lt;Window&gt;, bool)"/> constructor.
		/// <code>
		/// Window ReturnWindow()
		/// {
		///     return window; // your Avalonia application window where you want the login to pop up
		/// }
		/// var browser = new WebBrowser(ReturnWindow, shouldCloseWindow: false); // specify false if you want the window to remain open
		/// </code>
		/// </example>
		public WebBrowser(Func<Window> windowFactory, bool shouldCloseWindow = true)
		{
			_windowFactory = windowFactory;
			_shouldCloseWindow = shouldCloseWindow;
		}

		/// <summary>
		/// Create a new instance of <see cref="WebBrowser"/> that will create a customized <see cref="Window"/> as needed.
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
		/// <param name="systemDecorations">An optional <see cref="SystemDecorations"/> specifying the form system decorations.</param>
		/// <param name="canResize">An optional <see cref="bool"/> specifying the can resize option.</param>
		/// <param name="parent">An optional <see cref="int"/> specifying the parent form.</param>
		public WebBrowser(string title = "Authenticating...",
			double width = 1024, double height = 768,
			double minWidth = 1024, double minHeight = 768,
			double maxWidth = 1024, double maxHeight = 768,
			WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterScreen, 
			WindowIcon icon = null, 
			SystemDecorations systemDecorations = SystemDecorations.Full,
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
				SystemDecorations = systemDecorations,
				CanResize = canResize
			})
		{
			_parent = parent;
		}
		
		public WebBrowser(Window child, Window parent = null) : this(() => child)
		{
			_parent = parent;
		}

		/// <inheritdoc />
		public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
		{
			var window = _windowFactory.Invoke();
			var browser = new WebView();

			var signal = new SemaphoreSlim(0, 1);

			var result = new BrowserResult
			{
				ResultType = BrowserResultType.UserCancel
			};

			window.Closed += (_, _) => { signal.Release(); };

			browser.NavigationStarting += (_, arg) =>
			{
				if (!(arg?.Url?.AbsoluteUri.StartsWith(options.EndUrl) ?? false)) return;
				result.ResultType = BrowserResultType.Success;
				result.Response = arg.Url.ToString();
				if (_shouldCloseWindow)
				{
					window.Close();
				}
				else
				{
					window.Content = null;
				}
			};

			window.Content = browser;
			browser.Url = new Uri(options.StartUrl);

			if (_parent != null)
			{
				await window.ShowDialog(_parent);
			}
			else
			{
				window.Show();
				await signal.WaitAsync(cancellationToken);
			}

			return result;
		}
	}
}
