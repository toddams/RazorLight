// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.CodeGenerators.Visitors;

namespace RazorLight.Host
{
    public class LightCSharpCodeGenerator : CSharpCodeGenerator
    {
        private readonly string _defaultModel;

        public LightCSharpCodeGenerator(
            CodeGeneratorContext context,
            string defaultModel)
            : base(context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (defaultModel == null)
            {
                throw new ArgumentNullException(nameof(defaultModel));
            }

            _defaultModel = defaultModel;
        }

        protected override CSharpCodeVisitor CreateCSharpCodeVisitor(
            CSharpCodeWriter writer,
            CodeGeneratorContext context)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var csharpCodeVisitor = base.CreateCSharpCodeVisitor(writer, context);

            return csharpCodeVisitor;
        }

        protected override CSharpDesignTimeCodeVisitor CreateCSharpDesignTimeCodeVisitor(
            CSharpCodeVisitor csharpCodeVisitor,
            CSharpCodeWriter writer,
            CodeGeneratorContext context)
        {
            if (csharpCodeVisitor == null)
            {
                throw new ArgumentNullException(nameof(csharpCodeVisitor));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new LightCSharpDesignTimeCodeVisitor(csharpCodeVisitor, writer, context);
        }
    }
}