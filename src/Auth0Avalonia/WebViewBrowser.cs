using IdentityModel.OidcClient.Browser;
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Auth0.OidcClient
{
    /// <summary>
    /// Implements the <see cref="IBrowser"/> interface using the <see cref="WebViewCompatible"/> control.
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
        public WebViewBrowser(Func<Window> windowFactory, bool shouldCloseWindow = true)
        {
            _windowFactory = windowFactory;
            _shouldCloseWindow = shouldCloseWindow;
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
        /// <param name="parent">An optional <see cref="int"/> specifying the parent form.</param>
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
                Icon = icon
            })
        {
	        _parent = parent;
        }

        /// <inheritdoc />
        public Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<BrowserResult>();

            var window = _windowFactory();
            var webView = new WebViewControl.WebView();
            window.Content = webView;

            webView.Navigated += (url, name) =>
            {
                var uri = new Uri(url);
                if (uri.AbsoluteUri.StartsWith(options.EndUrl))
                {
                    tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.Success, Response = uri.ToString() });
                    if (_shouldCloseWindow)
                        window.Close();
                    else
                        window.Content = null;
                }
            };

            window.Closing += (sender, e) =>
            {
                webView.Dispose();
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(new BrowserResult { ResultType = BrowserResultType.UserCancel });
            };

            if (_parent != null)
	            window.Show(_parent);
            else
	            window.Show();
            webView.Address = options.StartUrl;

            return tcs.Task;
        }
    }
}
