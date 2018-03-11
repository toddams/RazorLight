using System;

namespace RazorLight.ViewFeatures.Internal
{
	/// <summary>
	/// Provides cached values for "name" and "id" HTML attributes.
	/// </summary>
	public static class NameAndIdProvider
	{
		private static readonly object PreviousNameAndIdKey = typeof(PreviousNameAndId);

		/// <summary>
		/// Returns a valid HTML 4.01 "id" attribute value for an element with the given <paramref name="fullName"/>.
		/// </summary>
		/// <param name="PageContext">A <see cref="PageContext"/> instance for the current scope.</param>
		/// <param name="fullName">
		/// The fully-qualified expression name, ignoring the current model. Also the original HTML element name.
		/// </param>
		/// <param name="invalidCharReplacement">
		/// The <see cref="string"/> (normally a single <see cref="char"/>) to substitute for invalid characters in
		/// <paramref name="fullName"/>.
		/// </param>
		/// <returns>
		/// Valid HTML 4.01 "id" attribute value for an element with the given <paramref name="fullName"/>.
		/// </returns>
		/// <remarks>
		/// Similar to <see cref="TagBuilder.CreateSanitizedId"/> but caches value for repeated invocations.
		/// </remarks>
		public static string CreateSanitizedId(PageContext PageContext, string fullName, string invalidCharReplacement)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			if (invalidCharReplacement == null)
			{
				throw new ArgumentNullException(nameof(invalidCharReplacement));
			}

			if (string.IsNullOrEmpty(fullName))
			{
				return string.Empty;
			}

			// Check cache to avoid whatever TagBuilder.CreateSanitizedId() may do.
			var items = PageContext.HttpContext.Items;
			object previousNameAndIdObject;
			PreviousNameAndId previousNameAndId = null;
			if (items.TryGetValue(PreviousNameAndIdKey, out previousNameAndIdObject) &&
				(previousNameAndId = (PreviousNameAndId)previousNameAndIdObject) != null &&
				string.Equals(previousNameAndId.FullName, fullName, StringComparison.Ordinal))
			{
				return previousNameAndId.SanitizedId;
			}

			var sanitizedId = TagBuilder.CreateSanitizedId(fullName, invalidCharReplacement);

			if (previousNameAndId == null)
			{
				// Do not create a PreviousNameAndId when TagBuilder.CreateSanitizedId() only examined fullName.
				if (string.Equals(fullName, sanitizedId, StringComparison.Ordinal))
				{
					return sanitizedId;
				}

				previousNameAndId = new PreviousNameAndId();
				items[PreviousNameAndIdKey] = previousNameAndId;
			}

			previousNameAndId.FullName = fullName;
			previousNameAndId.SanitizedId = sanitizedId;

			return previousNameAndId.SanitizedId;
		}

		/// <summary>
		/// Adds a valid HTML 4.01 "id" attribute for an element with the given <paramref name="fullName"/>. Does
		/// nothing if <see cref="TagBuilder.Attributes"/> already contains an "id" attribute or the
		/// <paramref name="fullName"/> is <c>null</c> or empty.
		/// </summary>
		/// <param name="PageContext">A <see cref="PageContext"/> instance for the current scope.</param>
		/// <param name="tagBuilder">A <see cref="TagBuilder"/> instance that will contain the "id" attribute.</param>
		/// <param name="fullName">
		/// The fully-qualified expression name, ignoring the current model. Also the original HTML element name.
		/// </param>
		/// <param name="invalidCharReplacement">
		/// The <see cref="string"/> (normally a single <see cref="char"/>) to substitute for invalid characters in
		/// <paramref name="fullName"/>.
		/// </param>
		/// <remarks>
		/// Similar to <see cref="TagBuilder.GenerateId"/> but caches value for repeated invocations.
		/// </remarks>
		/// <seealso cref="CreateSanitizedId"/>
		public static void GenerateId(
			PageContext PageContext,
			TagBuilder tagBuilder,
			string fullName,
			string invalidCharReplacement)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			if (tagBuilder == null)
			{
				throw new ArgumentNullException(nameof(tagBuilder));
			}

			if (invalidCharReplacement == null)
			{
				throw new ArgumentNullException(nameof(invalidCharReplacement));
			}

			if (string.IsNullOrEmpty(fullName))
			{
				return;
			}

			if (!tagBuilder.Attributes.ContainsKey("id"))
			{
				var sanitizedId = CreateSanitizedId(PageContext, fullName, invalidCharReplacement);

				// Duplicate check for null or empty to cover the corner case where fullName contains only invalid
				// characters and invalidCharReplacement is empty.
				if (!string.IsNullOrEmpty(sanitizedId))
				{
					tagBuilder.Attributes["id"] = sanitizedId;
				}
			}
		}

		/// <summary>
		/// Returns the full HTML element name for the specified <paramref name="expression"/>.
		/// </summary>
		/// <param name="PageContext">A <see cref="PageContext"/> instance for the current scope.</param>
		/// <param name="expression">Expression name, relative to the current model.</param>
		/// <returns>Fully-qualified expression name for <paramref name="expression"/>.</returns>
		/// <remarks>
		/// Similar to <see cref="TemplateInfo.GetFullHtmlFieldName"/> but caches value for repeated invocations.
		/// </remarks>
		public static string GetFullHtmlFieldName(PageContext PageContext, string expression)
		{
			var htmlFieldPrefix = PageContext.ViewData.TemplateInfo.HtmlFieldPrefix;
			if (string.IsNullOrEmpty(expression))
			{
				return htmlFieldPrefix;
			}

			if (string.IsNullOrEmpty(htmlFieldPrefix))
			{
				return expression;
			}

			// Need to concatenate. See if we've already done that.
			var items = PageContext.HttpContext.Items;
			object previousNameAndIdObject;
			PreviousNameAndId previousNameAndId = null;
			if (items.TryGetValue(PreviousNameAndIdKey, out previousNameAndIdObject) &&
				(previousNameAndId = (PreviousNameAndId)previousNameAndIdObject) != null &&
				string.Equals(previousNameAndId.HtmlFieldPrefix, htmlFieldPrefix, StringComparison.Ordinal) &&
				string.Equals(previousNameAndId.Expression, expression, StringComparison.Ordinal))
			{
				return previousNameAndId.OutputFullName;
			}

			if (previousNameAndId == null)
			{
				previousNameAndId = new PreviousNameAndId();
				items[PreviousNameAndIdKey] = previousNameAndId;
			}

			previousNameAndId.HtmlFieldPrefix = htmlFieldPrefix;
			previousNameAndId.Expression = expression;
			if (expression.StartsWith("[", StringComparison.Ordinal))
			{
				// The expression might represent an indexer access, in which case  with a 'dot' would be invalid.
				previousNameAndId.OutputFullName = htmlFieldPrefix + expression;
			}
			else
			{
				previousNameAndId.OutputFullName = htmlFieldPrefix + "." + expression;
			}

			return previousNameAndId.OutputFullName;
		}

		private class PreviousNameAndId
		{
			// Cached ambient input for NameAndIdProvider.GetFullHtmlFieldName(). TemplateInfo.HtmlFieldPrefix may
			// change during the lifetime of a PageContext.
			public string HtmlFieldPrefix { get; set; }

			// Cached input for NameAndIdProvider.GetFullHtmlFieldName().
			public string Expression { get; set; }

			// Cached return value for NameAndIdProvider.GetFullHtmlFieldName().
			public string OutputFullName { get; set; }

			// Cached input for NameAndIdProvider.CreateSanitizedId(). Since IHtmlHelper.GenerateIdFromName() is
			// available to all, there is no guarantee this is equal to OutputFullName when CreateSanitizedId() is
			// called.
			public string FullName { get; set; }

			// Cached return value for NameAndIdProvider.CreateSanitizedId().
			public string SanitizedId { get; set; }
		}
	}
}
