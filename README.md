![](.editoricon.png)
# Auth0 SDK for .NET Avalonia applications

OIDC Client for .NET Desktop (Avalonia) applications.

[![NuGet version](https://img.shields.io/nuget/v/Auth0Avalonia.svg?style=flat)](https://www.nuget.org/packages/Auth0Avalonia/)
[![Build status](https://ci.appveyor.com/api/projects/status/v8vsja2iwgdcnpff?svg=true)](https://ci.appveyor.com/project/ennerperez/auth0-oidc-client-avalonia)
[![License](https://img.shields.io/:license-Apache2.0-blue.svg?style=flat)](https://opensource.org/licenses/Apache-2.0)

:books: [Documentation](#documentation) - :rocket: [Getting Started](#getting-started) - :computer: [API Reference](#api-reference)

This library makes use of the [IdentityModel/IdentityModel.OidcClient](https://github.com/IdentityModel/IdentityModel.OidcClient) library and uses code from the [IdentityModel/IdentityModel.OidcClient.Samples](https://github.com/IdentityModel/IdentityModel.OidcClient.Samples) repository to achieve browser integration.

## Documentation

- [SDK docs](https://auth0.github.io/auth0-oidc-client-net/documentation/intro.html) - explore the documentation for this SDK. 
- [Auth0 docs](https://www.auth0.com/docs) - explore our docs site and learn more about
- [WebView](https://github.com/MicroSugarDeveloperOrg/Avalonia.WebView) - explore the Avalonia Webview

## Getting started

### Installation
The SDK is available on [Nuget](https://www.nuget.org/packages?q=Auth0.OidcClient) for different platforms:

```
Install-Package Auth0Avalonia 
```

### Configure Auth0

Create a **Native Application** in the [Auth0 Dashboard](https://manage.auth0.com/#/applications).

> **If you're using an existing application**, verify that you have configured the following settings in your Native Application:
>
> - Click on the "Settings" tab of your application's page.
> - Ensure that "Token Endpoint Authentication Method" under "Application Properties" is set to "None"
> - Scroll down and click on the "Show Advanced Settings" link.
> - Under "Advanced Settings", click on the "OAuth" tab.
> - Ensure that "JsonWebToken Signature Algorithm" is set to `RS256` and that "OIDC Conformant" is enabled.

Next, configure the following URLs for your application under the "Application URIs" section of the "Settings" page:

- **Allowed Callback URLs**
- **Allowed Logout URLs**

> For the values for these URLs, please refer to the corresponding quickstart from our [documentation](#documentation).

Take note of the **Client ID** and **Domain** values under the "Basic Information" section. You'll need these values to configure the SDK.

### Configure the SDK
All platforms share the same interface, so you can use the following code to instantiate the `Auth0Client`:

```csharp
using Auth0.OidcClient;
// ...
var auth0Client = new Auth0Client(new Auth0ClientOptions
{
    Domain = "YOUR_AUTH0_DOMAIN",
    ClientId = "YOUR_AUTH0_CLIENT_ID"
});
```

### Configure the client

Make sure to include the WebView use in the initialization

#### Desktop

Add `WebView.Avalonia.Desktop` nuget package to your AvaloniaUI Desktop project:

```shell
dotnet add package WebView.Avalonia.Desktop
```

Edit `Program.cs` file for desktop:

```csharp

using Avalonia.WebView.Desktop; //<<---add this
...
public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace()
        .UseReactiveUI()
        .UseDesktopWebView();   //<<---add this
```

##### Windows

Create `app.manifest` file for Windows:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">
	<!-- This manifest is used on Windows only.
         Don't remove it as it might cause problems with window transparency and embeded controls.
         For more details visit https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests -->
	<assemblyIdentity type="win32" name="AvaloniaUI" version="1.0.0.0" processorArchitecture="*"/>
	<compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
		<application>
			<!-- Windows 10 and Windows 11 -->
			<supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
			<!-- Windows 8.1 -->
			<supportedOS Id="{1f676c76-80e1-4239-95bb-83d0f6d0da78}" />
			<!-- Windows 8 -->
			<supportedOS Id="{4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38}" />
			<!-- Windows 7 -->
			<supportedOS Id="{35138b9a-5d96-4fbd-8e2d-a2440225f93a}" />
			<!-- Windows Vista -->
			<supportedOS Id="{e2011457-1546-43c5-a5fe-008deee3d3f0}" />
		</application>
	</compatibility>
</assembly>
```
> Read [Application manifests](https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests) to find out about the creating windows manifest.
 
Include ApplicationManifest node in the csproj file.

```xml
<PropertyGroup>
    <ApplicationManifest>app.manifest</ApplicationManifest>
</PropertyGroup>
```
#### Android

Add `WebView.Avalonia.Android` nuget package to your AvaloniaUI Android project:

```shell
dotnet add package WebView.Avalonia.Android
```
 
Edit `SplashActivity.cs` file for Android:

```csharp

using Avalonia.WebView.Android; //<<---add this
...
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return base.CustomizeAppBuilder(builder)
        .UseReactiveUI()
        .UseAndroidWebView();  //<<---add this
}
```
#### iOS

Add `WebView.Avalonia.iOS` nuget package to your AvaloniaUI iOS project:

```shell
dotnet add package WebView.Avalonia.iOS
```

Edit `App.axaml.cs` file:

```csharp

using Avalonia.WebView.iOS; //<<---add this
...
protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
{
    return builder.UseReactiveUI()
        .UseIosWebView();           //<<---add this
}
```
## API reference
Read [the full API reference](https://auth0.github.io/auth0-oidc-client-net/api/Auth0.OidcClient.html) to find out about the public API's this SDK exposes.
