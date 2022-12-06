![](.editoricon.png)
# Auth0 SDK for .NET Avalonia applications

OIDC Client for .NET Desktop (Avalonia) applications.

[![NuGet version](https://img.shields.io/nuget/v/Auth0Avalonia.svg?style=flat)](https://www.nuget.org/packages/Auth0Avalonia/)
[![Build status](https://ci.appveyor.com/api/projects/status/v8vsja2iwgdcnpff?svg=true)](https://ci.appveyor.com/project/ennerperez/auth0-oidc-client-avalonia)
[![License](https://img.shields.io/:license-Apache2.0-blue.svg?style=flat)](https://opensource.org/licenses/Apache-2.0)

:books: [Documentation](#documentation) - :rocket: [Getting Started](#getting-started) - :computer: [API Reference](#api-reference) - :speech_balloon: [Feedback](#feedback)

This library makes use of the [IdentityModel/IdentityModel.OidcClient](https://github.com/IdentityModel/IdentityModel.OidcClient) library and uses code from the [IdentityModel/IdentityModel.OidcClient.Samples](https://github.com/IdentityModel/IdentityModel.OidcClient.Samples) repository to achieve browser integration.

## Documentation

- [SDK docs](https://auth0.github.io/auth0-oidc-client-net/documentation/intro.html) - explore the documentation for this SDK. 
- [Auth0 docs](https://www.auth0.com/docs) - explore our docs site and learn more about 

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

## API reference
Read [the full API reference](https://auth0.github.io/auth0-oidc-client-net/api/Auth0.OidcClient.html) to find out about the public API's this SDK exposes.
