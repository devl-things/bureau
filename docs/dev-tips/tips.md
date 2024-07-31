#Tips

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
