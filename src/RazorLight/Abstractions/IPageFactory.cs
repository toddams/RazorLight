using RazorLight.Templating;

namespace RazorLight.Abstractions
{
    public interface IPageFactoryProvider
    {
	    RazorPageFactoryResult CreateFactory(string relativePath);
    }
}
