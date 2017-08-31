using System.Dynamic;
using System.IO;

namespace RazorLight
{
    public class PageContext : IPageContext
    {
        private dynamic _viewBag;

        public PageContext()
        {
            _viewBag = new ExpandoObject();
        }

        public PageContext(ExpandoObject viewBag)
        {
            _viewBag = viewBag ?? new ExpandoObject();
        }

        public TextWriter Writer { get; set; }

        /// <summary>
        /// Gets the dynamic view bag.
        /// </summary>
        public dynamic ViewBag => _viewBag;
    }
}
