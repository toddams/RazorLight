using System;

namespace RazorLight.Razor
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RazorLightTemplateAttribute : Attribute
    {
        public RazorLightTemplateAttribute(string key, Type viewType)
        {
            Key = key;
            ViewType = viewType;
        }

        /// <summary>
        /// Gets the key of the view.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the view type.
        /// </summary>
        public Type ViewType { get; }
    }
}
