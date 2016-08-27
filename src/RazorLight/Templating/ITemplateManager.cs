namespace RazorLight.Templating
{
	public interface ITemplateManager
	{
		ITemplateSource Resolve(string key);
	}
}
