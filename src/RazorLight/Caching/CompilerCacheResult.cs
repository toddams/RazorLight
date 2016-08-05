using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Primitives;
using RazorLight.Compilation;

namespace RazorLight.Caching
{
	/// <summary>
	/// Result of <see cref="ICompilerCache"/>.
	/// </summary>
	public struct CompilerCacheResult
	{
		/// <summary>
		/// Initializes a new instance of <see cref="CompilerCacheResult"/> with the specified
		/// <see cref="Compilation.CompilationResult"/>.
		/// </summary>
		/// <param name="relativePath">Path of the view file relative to the application base.</param>
		/// <param name="compilationResult">The <see cref="Compilation.CompilationResult"/>.</param>
		public CompilerCacheResult(string relativePath, CompilationResult compilationResult)
			: this(relativePath, compilationResult, new IChangeToken[0])
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="CompilerCacheResult"/> with the specified
		/// <see cref="Compilation.CompilationResult"/>.
		/// </summary>
		/// <param name="relativePath">Path of the view file relative to the application base.</param>
		/// <param name="compilationResult">The <see cref="Compilation.CompilationResult"/>.</param>
		/// <param name="expirationTokens">One or more <see cref="IChangeToken"/> instances that indicate when
		/// this result has expired.</param>
		public CompilerCacheResult(string relativePath, CompilationResult compilationResult, IList<IChangeToken> expirationTokens)
		{
			if (expirationTokens == null)
			{
				throw new ArgumentNullException(nameof(expirationTokens));
			}

			ExpirationTokens = expirationTokens;
			Type compiledType = compilationResult.CompiledType;

			NewExpression newExpression = Expression.New(compiledType);

			PropertyInfo pathProperty = compiledType.GetProperty(nameof(TemplatePage.Path));

			MemberAssignment propertyBindExpression = Expression.Bind(pathProperty, Expression.Constant(relativePath));
			MemberInitExpression objectInitializeExpression = Expression.MemberInit(newExpression, propertyBindExpression);
			PageFactory = Expression
				.Lambda<Func<TemplatePage>>(objectInitializeExpression)
				.Compile();
		}

		/// <summary>
		/// Initializes a new instance of <see cref="CompilerCacheResult"/> for a file that could not be
		/// found in the file system.
		/// </summary>
		/// <param name="expirationTokens">One or more <see cref="IChangeToken"/> instances that indicate when
		/// this result has expired.</param>
		public CompilerCacheResult(IList<IChangeToken> expirationTokens)
		{
			if (expirationTokens == null)
			{
				throw new ArgumentNullException(nameof(expirationTokens));
			}

			ExpirationTokens = expirationTokens;
			PageFactory = null;
		}

		/// <summary>
		/// <see cref="IChangeToken"/> instances that indicate when this result has expired.
		/// </summary>
		public IList<IChangeToken> ExpirationTokens { get; }

		/// <summary>
		/// Gets a value that determines if the view was successfully found and compiled.
		/// </summary>
		public bool Success => PageFactory != null;

		/// <summary>
		/// Gets a delegate that creates an instance of the <see cref="IRazorPage"/>.
		/// </summary>
		public Func<TemplatePage> PageFactory { get; }

	}
}
