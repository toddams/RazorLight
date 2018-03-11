using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using RazorLight.ViewFeatures.Internal;
using RazorLight.ViewFeatures.Rendering;
using Resources = System.Resources;

namespace RazorLight.ViewFeatures
{
	public class DefaultHtmlGenerator
	{
		private const string HiddenListItem = @"<li style=""display:none""></li>";

		private static readonly MethodInfo ConvertEnumFromStringMethod =
			typeof(DefaultHtmlGenerator).GetTypeInfo().GetDeclaredMethod(nameof(ConvertEnumFromString));

		// See: (http://www.w3.org/TR/html5/forms.html#the-input-element)
		private static readonly string[] _placeholderInputTypes =
			new[] { "text", "search", "url", "tel", "email", "password", "number" };

		//private readonly IAntiforgery _antiforgery;
		private readonly IUrlHelperFactory _urlHelperFactory;
		private readonly HtmlEncoder _htmlEncoder;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultHtmlGenerator"/> class.
		/// </summary>
		/// <param name="antiforgery">The <see cref="IAntiforgery"/> instance which is used to generate antiforgery
		/// tokens.</param>
		/// <param name="optionsAccessor">The accessor for <see cref="MvcViewOptions"/>.</param>
		/// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
		/// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
		/// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
		/// <param name="validationAttributeProvider">The <see cref="ValidationHtmlAttributeProvider"/>.</param>
		public DefaultHtmlGenerator(
			//IAntiforgery antiforgery,
			IUrlHelperFactory urlHelperFactory,
			HtmlEncoder htmlEncoder)
		{
			//if (antiforgery == null)
			//{
			//	throw new ArgumentNullException(nameof(antiforgery));
			//}


			if (urlHelperFactory == null)
			{
				throw new ArgumentNullException(nameof(urlHelperFactory));
			}

			if (htmlEncoder == null)
			{
				throw new ArgumentNullException(nameof(htmlEncoder));
			}

			//_antiforgery = antiforgery;
			_urlHelperFactory = urlHelperFactory;
			_htmlEncoder = htmlEncoder;

			// Underscores are fine characters in id's.
			IdAttributeDotReplacement = null; //TODO: $ or $$ or !!!
		}

		/// <inheritdoc />
		public string IdAttributeDotReplacement { get; }

		/// <inheritdoc />
		public string Encode(string value)
		{
			return !string.IsNullOrEmpty(value) ? _htmlEncoder.Encode(value) : string.Empty;
		}

		/// <inheritdoc />
		public string Encode(object value)
		{
			return (value != null) ? _htmlEncoder.Encode(value.ToString()) : string.Empty;
		}

		/// <inheritdoc />
		public string FormatValue(object value, string format)
		{
			if (value == null)
			{
				return string.Empty;
			}

			if (string.IsNullOrEmpty(format))
			{
				return Convert.ToString(value, CultureInfo.CurrentCulture);
			}
			else
			{
				return string.Format(CultureInfo.CurrentCulture, format, value);
			}
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateActionLink(
			PageContext PageContext,
			string linkText,
			string actionName,
			string controllerName,
			string protocol,
			string hostname,
			string fragment,
			object routeValues,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			if (linkText == null)
			{
				throw new ArgumentNullException(nameof(linkText));
			}

			var urlHelper = _urlHelperFactory.GetUrlHelper(PageContext);
			var url = urlHelper.Action(actionName, controllerName, routeValues, protocol, hostname, fragment);
			return GenerateLink(linkText, url, htmlAttributes);
		}

		/// <inheritdoc />
		//public virtual IHtmlContent GenerateAntiforgery(PageContext PageContext)
		//{
		//	if (PageContext == null)
		//	{
		//		throw new ArgumentNullException(nameof(PageContext));
		//	}

		//	var formContext = PageContext.FormContext;
		//	if (formContext.CanRenderAtEndOfForm)
		//	{
		//		// Inside a BeginForm/BeginRouteForm or a <form> tag helper. So, the antiforgery token might have
		//		// already been created and appended to the 'end form' content (the AntiForgeryToken HTML helper does
		//		// this) OR the <form> tag helper might have already generated an antiforgery token.
		//		if (formContext.HasAntiforgeryToken)
		//		{
		//			return HtmlString.Empty;
		//		}

		//		formContext.HasAntiforgeryToken = true;
		//	}

		//	return _antiforgery.GetHtml(PageContext.HttpContext);
		//}

		/// <inheritdoc />
		public virtual TagBuilder GenerateCheckBox(
			PageContext PageContext,
			string expression,
			bool? isChecked,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
			if (isChecked.HasValue && htmlAttributeDictionary != null)
			{
				// Explicit isChecked value must override "checked" in dictionary.
				htmlAttributeDictionary.Remove("checked");
			}

			// Use ViewData only in CheckBox case (metadata null) and when the user didn't pass an isChecked value.
			return GenerateInput(
				PageContext,
				InputType.CheckBox,
				expression,
				value: "true",
				isChecked: isChecked ?? false,
				setId: true,
				isExplicitValue: false,
				format: null,
				htmlAttributes: htmlAttributeDictionary);
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateHiddenForCheckbox(
			PageContext PageContext,
			string expression)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			var tagBuilder = new TagBuilder("input");
			tagBuilder.MergeAttribute("type", GetInputTypeString(InputType.Hidden));
			tagBuilder.MergeAttribute("value", "false");
			tagBuilder.TagRenderMode = TagRenderMode.SelfClosing;

			var fullName = NameAndIdProvider.GetFullHtmlFieldName(PageContext, expression);
			if (!string.IsNullOrEmpty(fullName))
			{
				tagBuilder.MergeAttribute("name", fullName);
			}

			return tagBuilder;
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateForm(
			PageContext PageContext,
			string actionName,
			string controllerName,
			object routeValues,
			string method,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			var defaultMethod = false;
			if (string.IsNullOrEmpty(method))
			{
				defaultMethod = true;
			}
			else if (string.Equals(method, "post", StringComparison.OrdinalIgnoreCase))
			{
				defaultMethod = true;
			}

			string action;
			if (actionName == null && controllerName == null && routeValues == null && defaultMethod)
			{
				throw new InvalidOperationException("Action & Controller not defined");
			}
			else
			{
				var urlHelper = _urlHelperFactory.GetUrlHelper(PageContext);
				action = urlHelper.Action(action: actionName, controller: controllerName, values: routeValues);
			}

			return GenerateFormCore(PageContext, action, method, htmlAttributes);
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateHidden(
			PageContext PageContext,
			string expression,
			object value,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			// Special-case opaque values and arbitrary binary data.
			if (value is byte[] byteArrayValue)
			{
				value = Convert.ToBase64String(byteArrayValue);
			}

			var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
			return GenerateInput(
				PageContext,
				InputType.Hidden,
				expression,
				value,
				isChecked: false,
				setId: true,
				isExplicitValue: true,
				format: null,
				htmlAttributes: htmlAttributeDictionary);
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateLabel(
			PageContext PageContext,
			string expression,
			string labelText,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			string resolvedLabelText = labelText;
			if (resolvedLabelText == null && expression != null)
			{
				var index = expression.LastIndexOf('.');
				if (index == -1)
				{
					// Expression does not contain a dot separator.
					resolvedLabelText = expression;
				}
				else
				{
					resolvedLabelText = expression.Substring(index + 1);
				}
			}

			var tagBuilder = new TagBuilder("label");
			var fullName = NameAndIdProvider.GetFullHtmlFieldName(PageContext, expression);
			var idString = NameAndIdProvider.CreateSanitizedId(PageContext, fullName, IdAttributeDotReplacement);
			tagBuilder.Attributes.Add("for", idString);
			tagBuilder.InnerHtml.SetContent(resolvedLabelText);
			tagBuilder.MergeAttributes(GetHtmlAttributeDictionaryOrNull(htmlAttributes), replaceExisting: true);

			return tagBuilder;
		}

		/// <inheritdoc />
		public virtual TagBuilder GeneratePassword(
			PageContext PageContext,
			string expression,
			object value,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
			return GenerateInput(
				PageContext,
				InputType.Password,
				expression,
				value,
				isChecked: false,
				setId: true,
				isExplicitValue: true,
				format: null,
				htmlAttributes: htmlAttributeDictionary);
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateRadioButton(
			PageContext PageContext,
			string expression,
			object value,
			bool? isChecked,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
			
			// RadioButton() case. Do not override checked attribute if isChecked is implicit.
			if (!isChecked.HasValue &&
				(htmlAttributeDictionary == null || !htmlAttributeDictionary.ContainsKey("checked")))
			{
				// Note value may be null if isChecked is non-null.
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				// isChecked not provided nor found in the given attributes; fall back to view data.
				var valueString = Convert.ToString(value, CultureInfo.CurrentCulture);
				isChecked = string.Equals(
					EvalString(PageContext, expression),
					valueString,
					StringComparison.OrdinalIgnoreCase);
			}

			if (isChecked.HasValue && htmlAttributeDictionary != null)
			{
				// Explicit isChecked value must override "checked" in dictionary.
				htmlAttributeDictionary.Remove("checked");
			}

			return GenerateInput(
				PageContext,
				InputType.Radio,
				expression,
				value,
				isChecked: isChecked ?? false,
				setId: true,
				isExplicitValue: true,
				format: null,
				htmlAttributes: htmlAttributeDictionary);
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateSelect(
			PageContext PageContext,
			string optionLabel,
			string expression,
			IEnumerable<SelectListItem> selectList,
			bool allowMultiple,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			var fullName = NameAndIdProvider.GetFullHtmlFieldName(PageContext, expression);
			var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
			if (!IsFullNameValid(fullName, htmlAttributeDictionary))
			{
				throw new ArgumentException(
					$"Field name can not be nullOrEmpty", nameof(expression));
			}

			// If we got a null selectList, try to use ViewData to get the list of items.
			if (selectList == null)
			{
				selectList = GetSelectListItems(PageContext, expression);
			}

			// Convert each ListItem to an <option> tag and wrap them with <optgroup> if requested.
			var listItemBuilder = GenerateGroupsAndOptions(optionLabel, selectList);

			var tagBuilder = new TagBuilder("select");
			tagBuilder.InnerHtml.SetHtmlContent(listItemBuilder);
			tagBuilder.MergeAttributes(htmlAttributeDictionary);
			NameAndIdProvider.GenerateId(PageContext, tagBuilder, fullName, IdAttributeDotReplacement);
			if (!string.IsNullOrEmpty(fullName))
			{
				tagBuilder.MergeAttribute("name", fullName, replaceExisting: true);
			}

			if (allowMultiple)
			{
				tagBuilder.MergeAttribute("multiple", "multiple");
			}

			return tagBuilder;
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateTextArea(
			PageContext PageContext,
			string expression,
			int rows,
			int columns,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			if (rows < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rows));
			}

			if (columns < 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(columns));
			}

			var fullName = NameAndIdProvider.GetFullHtmlFieldName(PageContext, expression);
			var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
			if (!IsFullNameValid(fullName, htmlAttributeDictionary))
			{
				throw new ArgumentException(
					$"Field name can not be nullOrEmpty", nameof(expression));
			}

			PageContext.ViewData.ModelState.TryGetValue(fullName, out var entry);

			var value = string.Empty;
			if (entry != null && entry.AttemptedValue != null)
			{
				value = entry.AttemptedValue;
			}
			else if (modelExplorer.Model != null)
			{
				value = modelExplorer.Model.ToString();
			}

			var tagBuilder = new TagBuilder("textarea");
			NameAndIdProvider.GenerateId(PageContext, tagBuilder, fullName, IdAttributeDotReplacement);
			tagBuilder.MergeAttributes(htmlAttributeDictionary, replaceExisting: true);
			if (rows > 0)
			{
				tagBuilder.MergeAttribute("rows", rows.ToString(CultureInfo.InvariantCulture), replaceExisting: true);
			}

			if (columns > 0)
			{
				tagBuilder.MergeAttribute(
					"cols",
					columns.ToString(CultureInfo.InvariantCulture),
					replaceExisting: true);
			}

			if (!string.IsNullOrEmpty(fullName))
			{
				tagBuilder.MergeAttribute("name", fullName, replaceExisting: true);
			}

			AddPlaceholderAttribute(PageContext.ViewData, tagBuilder, modelExplorer, expression);

			// If there are any errors for a named field, we add this CSS attribute.
			if (entry != null && entry.Errors.Count > 0)
			{
				tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
			}

			// The first newline is always trimmed when a TextArea is rendered, so we add an extra one
			// in case the value being rendered is something like "\r\nHello"
			tagBuilder.InnerHtml.AppendLine();
			tagBuilder.InnerHtml.Append(value);

			return tagBuilder;
		}

		/// <inheritdoc />
		public virtual TagBuilder GenerateTextBox(
			PageContext PageContext,
			string expression,
			object value,
			string format,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			var htmlAttributeDictionary = GetHtmlAttributeDictionaryOrNull(htmlAttributes);
			return GenerateInput(
				PageContext,
				InputType.Text,
				expression,
				value,
				isChecked: false,
				setId: true,
				isExplicitValue: true,
				format: format,
				htmlAttributes: htmlAttributeDictionary);
		}


		internal static string EvalString(PageContext PageContext, string key, string format)
		{
			return Convert.ToString(PageContext.ViewData.Eval(key, format), CultureInfo.CurrentCulture);
		}

		/// <remarks>
		/// Not used directly in HtmlHelper. Exposed for use in DefaultDisplayTemplates.
		/// </remarks>
		internal static TagBuilder GenerateOption(SelectListItem item, string text)
		{
			return GenerateOption(item, text, item.Selected);
		}

		internal static TagBuilder GenerateOption(SelectListItem item, string text, bool selected)
		{
			var tagBuilder = new TagBuilder("option");
			tagBuilder.InnerHtml.SetContent(text);

			if (item.Value != null)
			{
				tagBuilder.Attributes["value"] = item.Value;
			}

			if (selected)
			{
				tagBuilder.Attributes["selected"] = "selected";
			}

			if (item.Disabled)
			{
				tagBuilder.Attributes["disabled"] = "disabled";
			}

			return tagBuilder;
		}

		/// <summary>
		/// Generate a &lt;form&gt; element.
		/// </summary>
		/// <param name="PageContext">A <see cref="PageContext"/> instance for the current scope.</param>
		/// <param name="action">The URL where the form-data should be submitted.</param>
		/// <param name="method">The HTTP method for processing the form, either GET or POST.</param>
		/// <param name="htmlAttributes">
		/// An <see cref="object"/> that contains the HTML attributes for the element. Alternatively, an
		/// <see cref="IDictionary{String, Object}"/> instance containing the HTML attributes.
		/// </param>
		/// <returns>
		/// A <see cref="TagBuilder"/> instance for the &lt;/form&gt; element.
		/// </returns>
		protected virtual TagBuilder GenerateFormCore(
			PageContext PageContext,
			string action,
			string method,
			object htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			var tagBuilder = new TagBuilder("form");
			tagBuilder.MergeAttributes(GetHtmlAttributeDictionaryOrNull(htmlAttributes));

			// action is implicitly generated from other parameters, so htmlAttributes take precedence.
			tagBuilder.MergeAttribute("action", action);

			if (string.IsNullOrEmpty(method))
			{
				// Occurs only when called from a tag helper.
				method = "post";
			}

			// For tag helpers, htmlAttributes will be null; replaceExisting value does not matter.
			// method is an explicit parameter to HTML helpers, so it takes precedence over the htmlAttributes.
			tagBuilder.MergeAttribute("method", method, replaceExisting: true);

			return tagBuilder;
		}

		protected virtual TagBuilder GenerateInput(
			PageContext PageContext,
			InputType inputType,
			string expression,
			object value,
			bool isChecked,
			bool setId,
			bool isExplicitValue,
			string format,
			IDictionary<string, object> htmlAttributes)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			// Not valid to use TextBoxForModel() and so on in a top-level view; would end up with an unnamed input
			// elements. But we support the *ForModel() methods in any lower-level template, once HtmlFieldPrefix is
			// non-empty.
			var fullName = NameAndIdProvider.GetFullHtmlFieldName(PageContext, expression);
			if (!IsFullNameValid(fullName, htmlAttributes))
			{
				throw new ArgumentException(
					$"Field name can not be nullOrEmpty", nameof(expression));
			}

			var inputTypeString = GetInputTypeString(inputType);
			var tagBuilder = new TagBuilder("input")
			{
				TagRenderMode = TagRenderMode.SelfClosing,
			};

			tagBuilder.MergeAttributes(htmlAttributes);
			tagBuilder.MergeAttribute("type", inputTypeString);
			if (!string.IsNullOrEmpty(fullName))
			{
				tagBuilder.MergeAttribute("name", fullName, replaceExisting: true);
			}

			string suppliedTypeString = tagBuilder.Attributes["type"];
			string valueParameter = FormatValue(value, format);

			if (setId)
			{
				NameAndIdProvider.GenerateId(PageContext, tagBuilder, fullName, IdAttributeDotReplacement);
			}

			return tagBuilder;
		}

		protected virtual TagBuilder GenerateLink(
			string linkText,
			string url,
			object htmlAttributes)
		{
			if (linkText == null)
			{
				throw new ArgumentNullException(nameof(linkText));
			}

			var tagBuilder = new TagBuilder("a");
			tagBuilder.InnerHtml.SetContent(linkText);

			tagBuilder.MergeAttributes(GetHtmlAttributeDictionaryOrNull(htmlAttributes));
			tagBuilder.MergeAttribute("href", url);

			return tagBuilder;
		}


		private static Enum ConvertEnumFromInteger(object value, Type targetType)
		{
			try
			{
				return (Enum)Enum.ToObject(targetType, value);
			}
			catch (Exception exception)
			when (exception is FormatException || exception.InnerException is FormatException)
			{
				// The integer was too large for this enum type.
				return null;
			}
		}

		private static object ConvertEnumFromString<TEnum>(string value) where TEnum : struct
		{
			if (Enum.TryParse(value, out TEnum enumValue))
			{
				return enumValue;
			}

			// Do not return default(TEnum) when parse was unsuccessful.
			return null;
		}

		private static bool EvalBoolean(PageContext PageContext, string key)
		{
			return Convert.ToBoolean(PageContext.ViewData.Eval(key), CultureInfo.InvariantCulture);
		}

		private static string EvalString(PageContext PageContext, string key)
		{
			return Convert.ToString(PageContext.ViewData.Eval(key), CultureInfo.CurrentCulture);
		}

		// Only need a dictionary if htmlAttributes is non-null. TagBuilder.MergeAttributes() is fine with null.
		private static IDictionary<string, object> GetHtmlAttributeDictionaryOrNull(object htmlAttributes)
		{
			IDictionary<string, object> htmlAttributeDictionary = null;
			if (htmlAttributes != null)
			{
				htmlAttributeDictionary = htmlAttributes as IDictionary<string, object>;
				if (htmlAttributeDictionary == null)
				{
					htmlAttributeDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
				}
			}

			return htmlAttributeDictionary;
		}

		private static string GetInputTypeString(InputType inputType)
		{
			switch (inputType)
			{
				case InputType.CheckBox:
					return "checkbox";
				case InputType.Hidden:
					return "hidden";
				case InputType.Password:
					return "password";
				case InputType.Radio:
					return "radio";
				case InputType.Text:
					return "text";
				default:
					return "text";
			}
		}

		private static IEnumerable<SelectListItem> GetSelectListItems(
			PageContext PageContext,
			string expression)
		{
			if (PageContext == null)
			{
				throw new ArgumentNullException(nameof(PageContext));
			}

			// Method is called only if user did not pass a select list in. They must provide select list items in the
			// ViewData dictionary and definitely not as the Model. (Even if the Model datatype were correct, a
			// <select> element generated for a collection of SelectListItems would be useless.)
			var value = PageContext.ViewData.Eval(expression);

			// First check whether above evaluation was successful and did not match ViewData.Model.
			if (value == null || value == PageContext.ViewData.Model)
			{
				throw new InvalidOperationException(Resources.FormatHtmlHelper_MissingSelectData(
					$"IEnumerable<{nameof(SelectListItem)}>",
					expression));
			}

			// Second check the Eval() call returned a collection of SelectListItems.
			var selectList = value as IEnumerable<SelectListItem>;
			if (selectList == null)
			{
				throw new InvalidOperationException(Resources.FormatHtmlHelper_WrongSelectDataType(
					expression,
					value.GetType().FullName,
					$"IEnumerable<{nameof(SelectListItem)}>"));
			}

			return selectList;
		}

		private static bool IsFullNameValid(string fullName, IDictionary<string, object> htmlAttributeDictionary)
		{
			return IsFullNameValid(fullName, htmlAttributeDictionary, fallbackAttributeName: "name");
		}

		private static bool IsFullNameValid(
			string fullName,
			IDictionary<string, object> htmlAttributeDictionary,
			string fallbackAttributeName)
		{
			if (string.IsNullOrEmpty(fullName))
			{
				// fullName==null is normally an error because name="" is not valid in HTML 5.
				if (htmlAttributeDictionary == null)
				{
					return false;
				}

				// Check if user has provided an explicit name attribute.
				// Generalized a bit because other attributes e.g. data-valmsg-for refer to element names.
				htmlAttributeDictionary.TryGetValue(fallbackAttributeName, out var attributeObject);
				var attributeString = Convert.ToString(attributeObject, CultureInfo.InvariantCulture);
				if (string.IsNullOrEmpty(attributeString))
				{
					return false;
				}
			}

			return true;
		}

		/// <inheritdoc />
		public IHtmlContent GenerateGroupsAndOptions(string optionLabel, IEnumerable<SelectListItem> selectList)
		{
			return GenerateGroupsAndOptions(optionLabel, selectList, currentValues: null);
		}

		private IHtmlContent GenerateGroupsAndOptions(
			string optionLabel,
			IEnumerable<SelectListItem> selectList,
			ICollection<string> currentValues)
		{
			var itemsList = selectList as IList<SelectListItem>;
			if (itemsList == null)
			{
				itemsList = selectList.ToList();
			}

			var count = itemsList.Count;
			if (optionLabel != null)
			{
				count++;
			}

			// Short-circuit work below if there's nothing to add.
			if (count == 0)
			{
				return HtmlString.Empty;
			}

			var listItemBuilder = new HtmlContentBuilder(count);

			// Make optionLabel the first item that gets rendered.
			if (optionLabel != null)
			{
				listItemBuilder.AppendLine(GenerateOption(
					new SelectListItem()
					{
						Text = optionLabel,
						Value = string.Empty,
						Selected = false,
					},
					currentValues: null));
			}

			// Group items in the SelectList if requested.
			// The worst case complexity of this algorithm is O(number of groups*n).
			// If there aren't any groups, it is O(n) where n is number of items in the list.
			var optionGenerated = new bool[itemsList.Count];
			for (var i = 0; i < itemsList.Count; i++)
			{
				if (!optionGenerated[i])
				{
					var item = itemsList[i];
					var optGroup = item.Group;
					if (optGroup != null)
					{
						var groupBuilder = new TagBuilder("optgroup");
						if (optGroup.Name != null)
						{
							groupBuilder.MergeAttribute("label", optGroup.Name);
						}

						if (optGroup.Disabled)
						{
							groupBuilder.MergeAttribute("disabled", "disabled");
						}

						groupBuilder.InnerHtml.AppendLine();

						for (var j = i; j < itemsList.Count; j++)
						{
							var groupItem = itemsList[j];

							if (!optionGenerated[j] &&
								object.ReferenceEquals(optGroup, groupItem.Group))
							{
								groupBuilder.InnerHtml.AppendLine(GenerateOption(groupItem, currentValues));
								optionGenerated[j] = true;
							}
						}

						listItemBuilder.AppendLine(groupBuilder);
					}
					else
					{
						listItemBuilder.AppendLine(GenerateOption(item, currentValues));
						optionGenerated[i] = true;
					}
				}
			}

			return listItemBuilder;
		}

		private IHtmlContent GenerateOption(SelectListItem item, ICollection<string> currentValues)
		{
			var selected = item.Selected;
			if (currentValues != null)
			{
				var value = item.Value ?? item.Text;
				selected = currentValues.Contains(value);
			}

			var tagBuilder = GenerateOption(item, item.Text, selected);
			return tagBuilder;
		}
	}
}
