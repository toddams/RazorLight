# RazorLight

## Introduction
Install the nuget package (>=3.5.0).

	Install-Package RazorEngine


A lightweight fork of Microsoft Razor template engine which allows you to parse a string or *.cshtml file without any efforts. With RazorLight you can use Razor syntax to build dynamic templates. Аrom the very beginning library was built for [.NET Core](https://dotnet.github.io/) projects, but with a release of [.NET Platform Standard](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md) - it also supports full .NET Framework 4.6.2

##Example

```Csharp
string content = "Hello @Model.Name. Welcome to @Model.Title repository";

var model = new
{
  Name = "John Doe",
  Title = "RazorLight"
};

var engine = new RazorLightEngine();
string result = engine.ParseString(content, model);

//Output: Hello John Doe, Welcome to RazorLight repository
```
