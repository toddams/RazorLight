using System;
using Microsoft.DotNet.InternalAbstractions;

namespace RazorLight.Templating
{
	public struct ViewLocationCacheKey : IEquatable<ViewLocationCacheKey>
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ViewLocationCacheKey"/>.
		/// </summary>
		/// <param name="viewName">The view name or path.</param>
		/// <param name="isMainPage">Determines if the page being found is the main page for an action.</param>
		public ViewLocationCacheKey(
			string viewName,
			bool isMainPage)
		{
			ViewName = viewName;
			IsMainPage = isMainPage;
		}

		/// <summary>
		/// Gets the view name.
		/// </summary>
		public string ViewName { get; }


		/// <summary>
		/// Determines if the page being found is the main page for an action.
		/// </summary>
		public bool IsMainPage { get; }


		/// <inheritdoc />
		public bool Equals(ViewLocationCacheKey y)
		{
			if (IsMainPage != y.IsMainPage ||
				!string.Equals(ViewName, y.ViewName, StringComparison.Ordinal))
			{
				return false;
			}

			return true;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (obj is ViewLocationCacheKey)
			{
				return Equals((ViewLocationCacheKey)obj);
			}

			return false;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			var hashCodeCombiner = HashCodeCombiner.Start();
			hashCodeCombiner.Add(IsMainPage ? 1 : 0);
			hashCodeCombiner.Add(ViewName, StringComparer.Ordinal);


			return hashCodeCombiner.CombinedHash;
		}
	}
}
