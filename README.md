# RazorLight

## Introduction
Install the nuget package

	Install-Package RazorEngine


A lightweight fork of Microsoft [Razor template engine](https://github.com/aspnet/Razor) which allows you to parse strings / files / embedded resources without any efforts. With RazorLight you can use Razor syntax to build dynamic templates. From the very beginning library was built for [.NET Core](https://dotnet.github.io/) projects, but with a release of [.NET Platform Standard](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md) - it is also possible to use it on full .NET Framework (>= 4.6.2)

## Features
* Pass a model to your views (aka @model MyTestViewModel)
* Parse strings / files / embedded resources
* Layout page (like in ASP.NET MVC)
* ViewStart page (like in ASP.NET MVC)
* Custom namespaces (like _ViewStart)
* Templates are compiled once and cached. Next time you parse a template with the same key - you'll get it from the cache
* Templates are recompiled when RazorLight detects that template file was changed
* Create a custom ITemplateManager to resolve templates for ex. from your database


## Examples

### Parse files
In real world scenario you have a special folder where you store all your views files. You might also want to have some caching enabled. When you parse a file, RazorLight compiles a view and result is put into MemoryCache. Files in cache are tracked by FileWatcher, so you can safely modify your views. Once it detects that file was changed - template will be automatically recompiled and cached again.

```Csharp
var engine = EngineFactory.CreatePhysical(@"D:\path\to\views\folder");

var model = new TestModel()
{
	Title = "Hello, world",
	Description = "Some text here"
};

string result = engine.Parse("Test.cshtml", model);

```

*Note:* if you specify a model directly in a file and pass an anonymous object as a model parameter - you will get an ```InvalidCastException```


### Parse strings
```Csharp
string content = "Hello @Model.Name. Welcome to @Model.Title repository";

var model = new
{
  Name = "John Doe",
  Title = "RazorLight"
};

string result = engine.ParseString(content, model); //Output: Hello John Doe, Welcome to RazorLight repository
```

*Note:* when you parse a string - result is not cached

## FAQ
### I'm getting "Can't load metadata reference from the entry assembly" exception
Just set ```preserveCompilationContext": true``` in your project.json
