// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution.ApiSets;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    internal class DefaultRazorCSharpLoweringPhase : RazorEnginePhaseBase
    {
        internal static readonly object NewLineString = "NewLineString";

        internal static readonly object SuppressUniqueIds = "SuppressUniqueIds";

        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            var irDocument = codeDocument.GetIRDocument();
            ThrowForMissingDependency(irDocument);

            var syntaxTree = codeDocument.GetSyntaxTree();
            ThrowForMissingDependency(syntaxTree);

            var apiSet = ApiSet.Create(new BasicRazorApi(), new TagHelperApiExtension(), new TemplateApiExtension());

            var context = new CSharpWritingContext(apiSet, codeDocument, syntaxTree.Options);

            var idValue = codeDocument.Items[SuppressUniqueIds];
            if (idValue != null)
            {
                // Prevent generation of random IDs for test purposes.
                context.IdGenerator = () => idValue.ToString();
            }

            var newLineString = codeDocument.Items[NewLineString];
            if (newLineString != null)
            {
                // Set new line character to a specific string regardless of platform, for testing purposes.
                context.Writer.NewLine = (string)newLineString;
            }

            apiSet.WriteDocument(context, irDocument);

            var combinedErrors = syntaxTree.Diagnostics.Concat(context.Errors.Errors).ToList();
            var csharpDocument = new RazorCSharpDocument()
            {
                GeneratedCode = context.Writer.GenerateCode(),
                LineMappings = context.LineMappings.ToArray(),
                Diagnostics = combinedErrors
            };

            codeDocument.SetCSharpDocument(csharpDocument);
        }
    }
}