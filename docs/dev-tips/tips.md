#Tips
## Postgres

## WIP
- refresh token 
[Google implementation](https://github.com/googleapis/google-api-dotnet-client/blob/main/Src/Support/Google.Apis.Auth.AspNetCore3/GoogleAuthProvider.cs)
[Microsoft expires at](https://github.com/dotnet/aspnetcore/blob/562d119ca4a4275359f6fae359120a2459cd39e9/src/Security/Authentication/OpenIdConnect/src/OpenIdConnectHandler.cs#L940)

## Data migrations in different project
To add migration
```bash
C:\030220_repos\bureau\src\Bureau.Identity> dotnet ef migrations add Creation -o .\Data\Migrations -p .\ -s ..\Bureau.UI.Web\
```
To remove
```bash
C:\030220_repos\bureau\src\Bureau.Identity> dotnet ef migrations remove -p .\ -s ..\Bureau.UI.Web
```
To update database
```bash
C:\030220_repos\bureau\src\Bureau.Identity> dotnet ef database update -p .\ -s ..\Bureau.UI.Web
```

`-o` is output directory for migrations <br />
`-p` means project and doesn't need to have some extra packages <br />
`-s` means startup project and that onw need to have `Microsoft.EntityFrameworkCore.Design` package installed


## Page directive
Use constants in defining page directive properties
[Link](https://stackoverflow.com/questions/66894753/blazor-page-route-url-define-with-variable)
```csharp
@page "/Account/Login"
```
Same as
```csharp
@attribute [Route(UriPageAddresses.Login)]
```

## Prerendering
Life-cycle methods are by default ran twice because of prerendering
[Link](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-8.0#prerendering)
To disable prerendering
```csharp
@rendermode @(new InteractiveServerRenderMode(prerender: false))
```

## Authentication
https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-8.0#authentication
HttpContext can be used as a cascading parameter only in statically-rendered root components for general tasks, 
such as inspecting and modifying headers or other properties in the App component (Components/App.razor). The value is always null for interactive rendering.
```csharp
[CascadingParameter]
public HttpContext? HttpContext { get; set; }
```
For scenarios where the HttpContext is required in interactive components, we recommend flowing the data via persistent component state from the server. 
For more information, see [Server-side ASP.NET Core Blazor additional security scenarios](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/server/additional-scenarios?view=aspnetcore-8.0#pass-tokens-to-a-server-side-blazor-app).