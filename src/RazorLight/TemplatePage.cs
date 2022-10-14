using Microsoft.AspNetCore.Html;
using RazorLight.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorLight
{
	public abstract class TemplatePage : TemplatePageBase
	{
		private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private bool _renderedBody;
		private bool _ignoreBody;
		private HashSet<string> _ignoredSections;

		public async Task IncludeAsync(string key, object model = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (IncludeFunc == null)
			{
				throw new InvalidOperationException(nameof(IncludeFunc) + " is not set");
			}

			await IncludeFunc(key, model);
		}

		/// <summary>
		/// In a Razor layout page, renders the portion of a content page that is not within a named section.
		/// </summary>
		/// <returns>The HTML content to render.</returns>
		protected virtual IHtmlContent RenderBody()
		{
			if (BodyContent == null)
			{
				throw new InvalidOperationException("Method can not be called");
			}

			_renderedBody = true;
			return BodyContent;
		}

		/// <summary>
		/// In a Razor layout page, ignores rendering the portion of a content page that is not within a named section.
		/// </summary>
		public void IgnoreBody()
		{
			_ignoreBody = true;
		}

		#region Sections

		/// <summary>
		/// Creates a named content section in the page that can be invoked in a Layout page using
		/// <see cref="RenderSection(string)"/> or <see cref="RenderSectionAsync(string, bool)"/>.
		/// </summary>
		/// <param name="name">The name of the section to create.</param>
		/// <param name="section">The <see cref="RenderAsyncDelegate"/> to execute when rendering the section.</param>
		public override void DefineSection(string name, RenderAsyncDelegate section)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (section == null)
			{
				throw new ArgumentNullException(nameof(section));
			}

			if (SectionWriters.ContainsKey(name))
			{
				throw new InvalidOperationException($"Section {name} is already defined");
			}
			SectionWriters[name] = section;
		}

		/// <summary>
		/// Returns a value that indicates whether the specified section is defined in the content page.
		/// </summary>
		/// <param name="name">The section name to search for.</param>
		/// <returns><c>true</c> if the specified section is defined in the content page; otherwise, <c>false</c>.</returns>
		public bool IsSectionDefined(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			EnsureMethodCanBeInvoked(nameof(IsSectionDefined));
			return PreviousSectionWriters.ContainsKey(name);
		}

		/// <summary>
		/// In layout pages, renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the section to render.</param>
		/// <returns>An empty <see cref="IHtmlContent"/>.</returns>
		/// <remarks>The method writes to the <see cref="TemplatePageBase.Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		public HtmlString RenderSection(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			return RenderSection(name, required: true);
		}

		/// <summary>
		/// In layout pages, renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <param name="required">Indicates if this section must be rendered.</param>
		/// <returns>An empty <see cref="IHtmlContent"/>.</returns>
		/// <remarks>The method writes to the <see cref="TemplatePageBase.Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		public HtmlString RenderSection(string name, bool required)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			EnsureMethodCanBeInvoked(nameof(RenderSection));

			var task = RenderSectionAsyncCore(name, required);
			return task.GetAwaiter().GetResult();
		}

		/// <summary>
		/// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <returns>
		/// A <see cref="Task{HtmlString}"/> that on completion returns an empty <see cref="IHtmlContent"/>.
		/// </returns>
		/// <remarks>The method writes to the <see cref="TemplatePageBase.Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		public Task<HtmlString> RenderSectionAsync(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			return RenderSectionAsync(name, required: true);
		}

		/// <summary>
		/// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The section to render.</param>
		/// <param name="required">Indicates the <paramref name="name"/> section must be registered
		/// (using <c>@section</c>) in the page.</param>
		/// <returns>
		/// A <see cref="Task{HtmlString}"/> that on completion returns an empty <see cref="IHtmlContent"/>.
		/// </returns>
		/// <remarks>The method writes to the <see cref="TemplatePageBase.Output"/> and the value returned is a token
		/// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
		/// value does not represent the rendered content.</remarks>
		/// <exception cref="InvalidOperationException">if <paramref name="required"/> is <c>true</c> and the section
		/// was not registered using the <c>@section</c> in the Razor page.</exception>
		public Task<HtmlString> RenderSectionAsync(string name, bool required)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			EnsureMethodCanBeInvoked(nameof(RenderSectionAsync));
			return RenderSectionAsyncCore(name, required);
		}

		private async Task<HtmlString> RenderSectionAsyncCore(string sectionName, bool required)
		{
			if (_renderedSections.Contains(sectionName))
			{
				throw new InvalidOperationException($"Section {sectionName} is already rendered");
			}

			if (PreviousSectionWriters.TryGetValue(sectionName, out var renderDelegate))
			{
				_renderedSections.Add(sectionName);

				await renderDelegate();

				// Return a token value that allows the Write call that wraps the RenderSection \ RenderSectionAsync
				// to succeed.
				return HtmlString.Empty;
			}
			else if (required)
			{
				throw new InvalidOperationException($"Section {sectionName} is not defined");
			}
			else
			{
				// If the section is optional and not found, then don't do anything.
				return null;
			}
		}

		/// <summary>
		/// In layout pages, ignores rendering the content of the section named <paramref name="sectionName"/>.
		/// </summary>
		/// <param name="sectionName">The section to ignore.</param>
		public void IgnoreSection(string sectionName)
		{
			if (sectionName == null)
			{
				throw new ArgumentNullException(nameof(sectionName));
			}

			if (!PreviousSectionWriters.ContainsKey(sectionName))
			{
				throw new InvalidOperationException($"Section {sectionName} is not defined");
			}

			if (_ignoredSections == null)
			{
				_ignoredSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}

			_ignoredSections.Add(sectionName);
		}

		/// <inheritdoc />
		public override void EnsureRenderedBodyOrSections()
		{
			// a) all sections defined for this page are rendered.
			// b) if no sections are defined, then the body is rendered if it's available.
			if (PreviousSectionWriters != null && PreviousSectionWriters.Count > 0)
			{
				var sectionsNotRendered = PreviousSectionWriters.Keys.Except(
					_renderedSections,
					StringComparer.OrdinalIgnoreCase);

				string[] sectionsNotIgnored;
				if (_ignoredSections != null)
				{
					sectionsNotIgnored = sectionsNotRendered.Except(_ignoredSections, StringComparer.OrdinalIgnoreCase).ToArray();
				}
				else
				{
					sectionsNotIgnored = sectionsNotRendered.ToArray();
				}

				if (sectionsNotIgnored.Length > 0)
				{
					var sectionNames = string.Join(", ", sectionsNotIgnored);
					throw new InvalidOperationException($"One or more section(s) have been ignored. Ignored section(s): '{sectionNames}'");
				}
			}
			else if (BodyContent != null && !_renderedBody && !_ignoreBody)
			{
				// There are no sections defined, but RenderBody was NOT called.
				// If a body was defined and the body not ignored, then RenderBody should have been called.
				throw new InvalidOperationException("RenderBody was not called");
			}
		}

		public override void BeginContext(int position, int length, bool isLiteral)
		{
		}

		public override void EndContext()
		{
		}

		private void EnsureMethodCanBeInvoked(string methodName)
		{
			if (PreviousSectionWriters == null)
			{
				throw new InvalidOperationException($"Method {methodName} can not be called");
			}
		}

		#endregion
	}
}