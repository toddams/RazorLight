# RazorLight 

Use Razor to build templates from Files / EmbeddedResources / Strings / Database or your custom source outside of ASP.NET MVC. No redundant dependenies and workarounds in pair with excellent performance and NET Standard 2.0 support

[![Build Status](https://travis-ci.org/toddams/RazorLight.svg?branch=master)](https://travis-ci.org/toddams/RazorLight)  [![NuGet Pre Release](https://img.shields.io/nuget/vpre/RazorLight.svg?maxAge=2592000?style=flat-square)](https://www.nuget.org/packages/RazorLight/) [![Join the chat at https://gitter.im/gitterHQ/gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Razor-Light)

# Features
| Feature | RazorLight | ASP.NET MVC|
| --- | :---: | :---: |
| Model | :heavy_check_mark: | :heavy_check_mark: |
| Layout | :heavy_check_mark: | :heavy_check_mark: |
| Sections | :heavy_check_mark: | :heavy_check_mark: |
| Partials | :heavy_check_mark: | :heavy_check_mark: |
| @Inject | :heavy_check_mark: | :heavy_check_mark: |
| @Inherits | :heavy_check_mark: | :heavy_check_mark: |
| String source | :heavy_check_mark: | :x: |
| ViewComponents | In progress | :heavy_check_mark: |
| Url Helpers | In progress | :heavy_check_mark: |
| Tag Helpers | In progress | :heavy_check_mark: |


# Quickstart
Install the nuget package using following command:

````
Install-Package RazorLight -Version 2.0.0-alpha3
````

RazorLight uses Microsoft's Razor parsing engine which you may already know from ASP.NET MVC, where it is a default templating engine. 
````CSharp
using RazorLight;

string template = "Hello, @Model.Name. Welcome to RazorLight repository";
var model = new { Name = "John Doe" };

IRazorLihtEngine engine = new EngineFactory().Create();

string result = await engine.CompileRenderAsync("templateKey", template, model);
````
The ````templateKey```` must be unique value that identifies your template and after running the above example you can render the cached template again with this key. 

In example above we use an anonymous type as a template model for simplicity, but it's recommended to create a separate ViewModel for each template. When you pass anonymous type as a model - template model type will be marked as ````dynamic```` - as a result performance will be slightly worse.


````CSharp
string template = "Hello, @Model.Name. Welcome to RazorLight repository";
string result = await engine.CompileRenderAsync("templateKey", template, new UserViewModel() { Name = "John Doe" });
````

Once you compile the template - it is cached and you can render it again avoiding recompilation.
````Csharp
//Assume template with this key was rendered before
var cacheResult = engine.TemplateCache.RetrieveTemplate("templateKey");
if(cacheResult.Success)
{
    string result = await engine.RenderTemplateAsync(cacheResult.Template, new TestViewModel(), typeof(TestViewModel));
}
````

# Template sources
RazorLight can resolve templates from any source, but there are a built-in providers that resolve template source from filesystem and embedded resources.

## File source
````CSharp
var engine = new EngineFactory().ForFileSystem("C:/RootFolder/With/YourTemplates");

string result = await engine.CompileRenderAsync("Subfolder/View.cshtml", new { Name = "John Doe" });
````
When resolving a template from filesystem, templateKey - is a relative path to the root folder, that you pass to engineFactory constructor.

## EmbeddedResource source
````CSharp
var engine = new EngineFactory().ForEmbeddedResources(rootType: typeof(Root))

string result = await engine.CompileRenderAsync("Templates.View.cshtml", new { Name = "John Doe" });
````
For embedded resource, key - is a namespace and key of the embedded resource relative to root Type. Then root type namespace and templateKey will be combined into YourAssembly.NamespaceOfRootType.Templates.View.cshtml

## Custom source
If you store your templates in database - it is recommended to create custom RazorLightProject that is responsible for gettings templages source from it.
The class will be used to get template source and ViewImports. RazorLight will use it to resolve Layouts, when you specify it inside the template.

````CSharp
var project = new EntityFrameworkRazorProject(new AppDbContext());
var engine = new EngineFactoty().Create(project);

// For key as a GUID
string result = await engine.CompileRenderAsync("6cc277d5-253e-48e0-8a9a-8fe3cae17e5b", new { Name = "John Doe" });

// Or integer
int templateKey = 322;
string result = await engine.CompileRenderAsync(templateKey.ToString(), new { Name = "John Doe" });

````
You can find a full sample [here](https://github.com/toddams/RazorLight/tree/dev-2.0/samples/RazorLight.Samples)

# Includes (aka Partial views)
Include feature is useful when you have reusable parts of your templates you want to share between different views. Includes are an effective way of breaking up large templates into smaller components. They can reduce duplication of template content and allow elements to be reused.

````CSharp
@model MyProject.TestViewModel
<div>
    Hello @Model.Title
</div>

@{ await IncludeAsync("SomeView.cshtml", Model); }
````
First argument takes a key of the template to resolve, second argument is a model of the view (can be null)

# Encoding
By the default RazorLight encodes Model values as HTML, but sometimes you want to output them as is. You can disable encoding for specific value using @Raw() function

````CSharp
/* With encoding (default) */

string template = "Render @Model.Tag";
string result = await engine.CompileRenderAsync("templateKey", template, new { Tag = "<html>&" });

Console.WriteLine(result); // Output: &lt;html&gt;&amp

/* Without encoding */

string template = "Render @Raw(Model.Tag)";
string result = await engine.CompileRenderAsync("templateKey", template, new { Tag = "<html>&" });

Console.WriteLine(result); // Output: <html>&
````
In order to disable encoding for the entire document - just set ````"DisableEncoding"```` variable to true
````html
@model TestViewModel
@{
    DisableEncoding = true;
}

<html>
    Hello @Model.Tag
</html>
````

# Additional metadata references
When RazorLight compiles your template - it loads all the assemblies from your entry assembly and creates MetadataReference from it. This is a default strategy and it works in 99% of the time. But sometimes compilation crashes with an exception message like "Can not find assembly My.Super.Assembly2000". In order to solve this problem you can pass additional metadata references to RazorLight.

````CSharp
var options = new RazorLightOptions();

var metadataReference = MetadataReference.CreateFromFile("path-to-your-assembly")
options.AdditionalMetadataReferences.Add(metadataReference );

var project = new FileSystemRazorProject("path-to-your-views");
var engine = new EngineFactory().Create(project, options);
````

# Enable Intellisense support
Visual Studio tooling knows nothing about RazorLight and assumes, that the view you are using - is a typical ASP.NET MVC template. In order to enable Intellisense for RazorLight templates, you should give Visual Studio a little hint about the base template class, that all your templates inherit implicitly

````CSharp
@using RazorLight
@inherits TemplatePage<MyModel>

<html>
    Your awesome template goes here, @Model.Name
</html>
````
____
![Intellisense](github/autocomplete.png)

# FAQ
## I'm getting "Can't load metadata reference from the entry assembly" exception

Set PreserveCompilationContext to true in your *.csproj file

````XML
<ItemGroup>
    <PreserveCompilationContext>true</PreserveCompilationContext>
</ItemGroup>
````