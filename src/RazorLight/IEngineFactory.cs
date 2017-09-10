namespace RazorLight
{
    public interface IEngineFactory
    {
        /// <summary>
        /// Creates RazorLightEngine with a filesystem razor project
        /// </summary>
        /// <param name="root">Root folder where views are stored</param>
        /// <returns></returns>
        RazorLightEngine ForFileSystem(string root);
    }
}