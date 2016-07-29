// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.Parser;

namespace RazorLight.Host
{
	public class RazorLightCodeParser : CSharpCodeParser
    {
        private const string ModelKeyword = "model";
        private SourceLocation? _endInheritsLocation;
        private bool _modelStatementFound;

        public RazorLightCodeParser()
        {
            MapDirectives(ModelDirective, ModelKeyword);
        }

        protected override void InheritsDirective()
        {
            // Verify we're on the right keyword and accept
            AssertDirective(SyntaxConstants.CSharp.InheritsKeyword);
            AcceptAndMoveNext();
            _endInheritsLocation = CurrentLocation;

            InheritsDirectiveCore();
            CheckForInheritsAndModelStatements();
        }

        private void CheckForInheritsAndModelStatements()
        {
            if (_modelStatementFound && _endInheritsLocation.HasValue)
            {
                Context.OnError(
                    _endInheritsLocation.Value,
					Resources.MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword,
                    SyntaxConstants.CSharp.InheritsKeyword.Length);
            }
        }

        protected virtual void ModelDirective()
        {
            // Verify we're on the right keyword and accept
            AssertDirective(ModelKeyword);
            var startModelLocation = CurrentLocation;
            AcceptAndMoveNext();


			BaseTypeDirective("Keyword must be followed with the type name",
							  CreateModelChunkGenerator);

			if (_modelStatementFound)
            {
                Context.OnError(
                    startModelLocation,
                    Resources.MvcRazorCodeParser_OnlyOneModelStatementIsAllowed,
                    ModelKeyword.Length);
            }

            _modelStatementFound = true;

            CheckForInheritsAndModelStatements();
        }

        private SpanChunkGenerator CreateModelChunkGenerator(string model)
        {
            return new ModelChunkGenerator(model);
        }

        // Internal for unit testing
        internal static string RemoveWhitespaceAndTrailingSemicolons(string value)
        {
            Debug.Assert(value != null);
            value = value.TrimStart();

            for (var index = value.Length - 1; index >= 0; index--)
            {
                var currentChar = value[index];
                if (!char.IsWhiteSpace(currentChar) && currentChar != ';')
                {
                    return value.Substring(0, index + 1);
                }
            }

            return string.Empty;
        }
    }
}
