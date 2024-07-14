using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Auth0.OidcClient;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaMVVM.ViewModels
{
	public partial class MainWindowViewModel : ViewModelBase
	{
		private readonly Func<Auth0Client> _auth0Client;
		private readonly Action<string> _writeLine;
		private readonly Action _clearText;
		private string _accessToken;
		private Auth0Client _client;

		public MainWindowViewModel()
		{
			_writeLine = (text) => OutputText += text + "\n";
			_clearText = () => OutputText = "";

			_auth0Client = () =>
			{
				if (_client != null) return _client;
				if (Application.Current == null || Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return null;
				var window = new Window()
				{
					Width = desktop.MainWindow?.Width ?? 480,
					Height = desktop.MainWindow?.Height ?? 640,
					MinWidth = desktop.MainWindow?.MinWidth ?? 480,
					MinHeight = desktop.MainWindow?.MinHeight ?? 640,
					MaxWidth = desktop.MainWindow?.Screens.Primary?.Bounds.Width ?? 1024,
					MaxHeight = desktop.MainWindow?.Screens.Primary?.Bounds.Height ?? 768,
					Icon = desktop.MainWindow?.Icon,
					WindowStartupLocation = WindowStartupLocation.CenterOwner,
					CanResize = false
				};
				_client = new Auth0Client(new Auth0ClientOptions
				{
					Domain = "auth0-dotnet-integration-tests.auth0.com",
					ClientId = "qmss9A66stPWTOXjR6X1OeA0DLadoNP2",
					Browser = new WebBrowser(window, desktop.MainWindow)
				});
				return _client;

			};

			LoginButtonCommand = new AsyncRelayCommand<RoutedEventArgs>(LoginButton_Click);
			UserInfoButtonCommand = new AsyncRelayCommand<RoutedEventArgs>(UserInfoButton_Click);
			LogoutButtonCommand = new AsyncRelayCommand<RoutedEventArgs>(LogoutButton_OnClick);
		}

		public ICommand LoginButtonCommand { get; set; }
		public ICommand UserInfoButtonCommand { get; set; }
		public ICommand LogoutButtonCommand { get; set; }

		[ObservableProperty] private string outputText;

		private async Task LoginButton_Click(RoutedEventArgs e)
		{
			_clearText();
			_writeLine("Starting login...");

			var loginResult = await _auth0Client().LoginAsync(new { organization = "" });

			if (loginResult.IsError)
			{
				_writeLine($"An error occurred during login: {loginResult.Error}");
				return;
			}

			_accessToken = loginResult.AccessToken;

			_writeLine($"id_token: {loginResult.IdentityToken}");
			_writeLine($"access_token: {loginResult.AccessToken}");
			_writeLine($"refresh_token: {loginResult.RefreshToken}");

			_writeLine($"name: {loginResult.User.FindFirst(c => c.Type == "name")?.Value}");
			_writeLine($"email: {loginResult.User.FindFirst(c => c.Type == "email")?.Value}");

			foreach (var claim in loginResult.User.Claims)
			{
				_writeLine($"{claim.Type} = {claim.Value}");
			}
		}

		private async Task UserInfoButton_Click(RoutedEventArgs e)
		{
			_clearText();

			if (string.IsNullOrEmpty(_accessToken))
			{
				_writeLine("You need to be logged in to get user info");
				return;
			}

			_writeLine("Getting user info...");
			var userInfoResult = await _auth0Client().GetUserInfoAsync(_accessToken);

			if (userInfoResult.IsError)
			{
				_writeLine($"An error occurred getting user info: {userInfoResult.Error}");
				return;
			}

			foreach (var claim in userInfoResult.Claims)
			{
				_writeLine($"{claim.Type} = {claim.Value}");
			}
		}

		private async Task LogoutButton_OnClick(RoutedEventArgs e)
		{
			_clearText();
			_writeLine("Starting logout...");

			var result = await _auth0Client().LogoutAsync();
			_accessToken = null;
			_writeLine(result.ToString());
		}
	}
}
