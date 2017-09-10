using System.IO;

namespace RazorLight.Razor
{
    public abstract class RazorLightProjectItem
    {
        /// <summary>
        /// Unique key of the template that was seached
        /// </summary>
        public abstract string Key { get; set; }

        /// <summary>
        /// Gets if template exists
        /// </summary>
        public abstract bool Exists { get; }


        /// <summary>
        /// Returns 
        /// </summary>
        /// <returns></returns>
        public abstract Stream Read();
    }
}
