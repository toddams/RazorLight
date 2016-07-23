# RazorLight

## Introduction
Install the nuget package

	Install-Package RazorEngine


A lightweight fork of Microsoft Razor template engine which allows you to parse a string or *.cshtml files without any efforts. With RazorLight you can use Razor syntax to build dynamic templates. From the very beginning library was built for [.NET Core](https://dotnet.github.io/) projects, but with a release of [.NET Platform Standard](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md) - it is also possible to use it on full .NET Framework (>= 4.6.2)


##Examples

### Parse string
You can use both anonymous objects and real models for your templates
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
### Parse files
In real world scenario you have a special folder where you store all your views files. You might also want to have some caching enabled. When you parse a file, RazorLight compiles a view and result is put into MemoryCache. Files in cache are tracked by FileWatcher, so you can safely modify your views. Once it detects that file was changed - template will be automatically recompiled and cached again.

```Csharp
var options = new ConfigurationOptions()
{
	ViewsFolder = @"D:\path\to\views\folder"
};
var engine = new RazorLightEngine(options);

var model = new TestModel()
{
	Title = "Hello, world",
	Description = "Some text here"
};

string result = engine.ParseFile("Test.cshtml", model);

```
*Note:* if you specify a model directly in a file and pass an anonymous object as a model parameter - you will get an ```InvalidCastException```

## FAQ
### I'm getting "Can't load metadata reference from the entry assembly" exception
Just set ```preserveCompilationContext": true``` in your project.json
