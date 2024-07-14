using System;
using Auth0.OidcClient;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaUI
{
	public partial class MainWindow : Window
	{
		private readonly Auth0Client _auth0Client;
		private readonly Action<string> _writeLine;
		private readonly Action _clearText;
		private string _accessToken;

		public MainWindow()
		{
			InitializeComponent();
			_writeLine = (text) => outputText.Text += text + "\n";
			_clearText = () => outputText.Text = "";

			var window = new Window()
			{
				Width = Width,
				Height = Height,
				MinWidth = MinWidth,
				MinHeight = MinHeight,
				MaxWidth = Screens.Primary?.Bounds.Width ?? 1024,
				MaxHeight = Screens.Primary?.Bounds.Height ?? 768,
				Icon = Icon,
				WindowStartupLocation = WindowStartupLocation.CenterOwner, 
				CanResize = false
			};

			_auth0Client = new Auth0Client(new Auth0ClientOptions
			{
				Domain = "auth0-dotnet-integration-tests.auth0.com",
				ClientId = "qmss9A66stPWTOXjR6X1OeA0DLadoNP2",
				Browser = new WebBrowser(window, this)
			});
		}

		private async void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			_clearText();
			_writeLine("Starting login...");

			var loginResult = await _auth0Client.LoginAsync(new { organization = "" });

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

		private async void LogoutButton_OnClick(object sender, RoutedEventArgs e)
		{
			_clearText();
			_writeLine("Starting logout...");

			var result = await _auth0Client.LogoutAsync();
			_accessToken = null;
			_writeLine(result.ToString());
		}

		private async void UserInfoButton_Click(object sender, RoutedEventArgs e)
		{
			_clearText();

			if (string.IsNullOrEmpty(_accessToken))
			{
				_writeLine("You need to be logged in to get user info");
				return;
			}

			_writeLine("Getting user info...");
			var userInfoResult = await _auth0Client.GetUserInfoAsync(_accessToken);

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
	}
}
