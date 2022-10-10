using Microsoft.AspNetCore.Razor.TagHelpers;
using RazorLight.Internal;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace RazorLight.TagHelpers
{
	/// <summary>
	/// Default implementation for <see cref="ITagHelperFactory"/>.
	/// </summary>
	public class DefaultTagHelperFactory : ITagHelperFactory
	{
		private readonly ITagHelperActivator _activator;
		private readonly ConcurrentDictionary<Type, PropertyActivator<PageContext>[]> _injectActions;
		//private readonly Func<Type, PropertyActivator<PageContext>[]> _getPropertiesToActivate;
		private static readonly Func<PropertyInfo, PropertyActivator<PageContext>> _createActivateInfo = CreateActivateInfo;

		/// <summary>
		/// Initializes a new <see cref="DefaultTagHelperFactory"/> instance.
		/// </summary>
		/// <param name="activator">
		/// The <see cref="ITagHelperActivator"/> used to create tag helper instances.
		/// </param>
		public DefaultTagHelperFactory(ITagHelperActivator activator)
		{
			if (activator == null)
			{
				throw new ArgumentNullException(nameof(activator));
			}

			_activator = activator;
			_injectActions = new ConcurrentDictionary<Type, PropertyActivator<PageContext>[]>();

			//TODO: ViewContextAttribute or PageContextAttribute
			//_getPropertiesToActivate = type =>
			//    PropertyActivator<PageContext>.GetPropertiesToActivate(
			//        type,
			//        typeof(ViewContextAttribute),
			//        _createActivateInfo);
		}

		/// <inheritdoc />
		public TTagHelper CreateTagHelper<TTagHelper>(PageContext context)
			where TTagHelper : ITagHelper
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var tagHelper = _activator.Create<TTagHelper>(context);

			var propertiesToActivate = _injectActions.GetOrAdd(
				tagHelper.GetType(),
				default(PropertyActivator<PageContext>[]));// _getPropertiesToActivate);

			for (var i = 0; i < propertiesToActivate.Length; i++)
			{
				var activateInfo = propertiesToActivate[i];
				activateInfo.Activate(tagHelper, context);
			}

			InitializeTagHelper(tagHelper, context);

			return tagHelper;
		}

		private static void InitializeTagHelper<TTagHelper>(TTagHelper tagHelper, PageContext context)
			where TTagHelper : ITagHelper
		{
			//TODO: replace IServiceCollection
			//var serviceProvider = context.HttpContext.RequestServices;
			//var initializers = serviceProvider.GetService<IEnumerable<ITagHelperInitializer<TTagHelper>>>();

			//foreach (var initializer in initializers)
			//{
			//    initializer.Initialize(tagHelper, context);
			//}
		}

		private static PropertyActivator<PageContext> CreateActivateInfo(PropertyInfo property)
		{
			return new PropertyActivator<PageContext>(property, viewContext => viewContext);
		}
	}
}
