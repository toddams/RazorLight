# RazorLight

| Build status | Nuget package |
| --- | --- |
| [![Build Status](https://travis-ci.org/toddams/RazorLight.svg?branch=master)](https://travis-ci.org/toddams/RazorLight)  [![Build status](https://ci.appveyor.com/api/projects/status/x4x8y0ue3w8cswhf?svg=true)](https://ci.appveyor.com/project/toddams/razorlight) | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/RazorLight.svg?maxAge=2592000?style=flat-square)](https://www.nuget.org/packages/RazorLight/) |


## Introduction
Install the nuget package

	Install-Package RazorLight -Pre


Use [Razor parsing engine](https://github.com/aspnet/Razor) to build dynamic templates from strings / files / embedded resources without any efforts. From the very beginning library was built for [.NET Core](https://dotnet.github.io/) projects (NetStandard 1.6), but now Full .NET Framework v4.5.1 (and higher) is also supported.

## Features
* Pass a model to your views (aka @model MyTestViewModel)
* Build templates from strings / files / embedded resources
* Layout pages, sections and ViewStart pages (like in ASP.NET MVC)
* ASP.NET MVC Integration ([RazorLight.MVC](https://www.nuget.org/packages/RazorLight.MVC/)). Inject services in your templates using @inject feature (Dependency Injection)
* Performance. After template is compiled - result is put in MemoryCache. Templates are recompiled when RazorLight detects that template file was changed
* Create a custom ITemplateManager to resolve templates for ex. from your database


## Examples

### Files
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


### Strings
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

### Embedded resources

Include your resource as embeded in *project.json*
````Javascript
"buildOptions": {
    "embed": [
      "Test.cshtml"
    ]
  },
````

Then create a RazorLightEngine for embedded resources

```CSharp
//typeof(TestViewModel) - root object inside your resource assembly
var engine = EngineFactory.CreateEmbedded(typeof(TestViewModel)) 

var model = new TestModel()
{
    Title = "Hello, world",
    Description = "Some text here"
};

//Note: pass the name of the view without extension
string result = engine.Parse("Test", model); 

````

## ASP.NET MVC Core integration
- **Add package**

    ````Install-Package RazorLight.MVC````

- **Add RazorLight services in Startup.cs**

````CSharp
public void ConfigureServices(IServiceCollection services)
{
     ....
    services.AddRazorLight("/Views"); // <- This one
     ....
}
````

- **Retreive IRazorLightEngine instance from controller constructor**
````CSharp
private readonly IRazorLightEngine engine;

public HomeController(IRazorLightEngine engine)
{
    this.engine = engine;
}
````
- **Inject services to your templates**
````CSharp
@inject MyProject.TestService myService
````

## FAQ
### I'm getting "Can't load metadata reference from the entry assembly" exception
Just set ```preserveCompilationContext": true``` in your project.json
