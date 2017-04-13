﻿using System.Collections.Generic;
using System.Dynamic;
using System.IO;

namespace RazorLight
{
	public class PageContext
	{
		private readonly dynamic viewBag;

		public PageContext()
		{
			viewBag = new ExpandoObject();
		}

		public PageContext(ExpandoObject viewBag)
		{
			this.viewBag = viewBag ?? new ExpandoObject();
		}

		/// <summary>
		/// Gets the current writer.
		/// </summary>
		/// <value>The writer.</value>
		public TextWriter Writer { get; internal set; }

		/// <summary>
		/// Gets the view bag.
		/// </summary>
		/// <value>The view bag.</value>
		public dynamic ViewBag => viewBag;

		public IList<ITemplatePage> ViewStartPages { get; } = new List<ITemplatePage>();

		/// <summary>
		/// Gets the info of the template model
		/// </summary>
		public ModelTypeInfo ModelTypeInfo { get; set; }

		public string ExecutingFilePath { get; set; }
	}
}
