// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution.CodeGeneration;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    internal class DefaultRazorCSharpLoweringPhase : RazorEnginePhaseBase, IRazorCSharpLoweringPhase
    {
        internal static readonly object NewLineString = "NewLineString";

        internal static readonly object SuppressUniqueIds = "SuppressUniqueIds";

        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            var irDocument = codeDocument.GetIRDocument();
            ThrowForMissingDependency(irDocument);

            var syntaxTree = codeDocument.GetSyntaxTree();
            ThrowForMissingDependency(syntaxTree);

            var target = irDocument.Target;
            if (target == null)
            {
                var message = Resources.FormatDocumentMissingTarget(
                    irDocument.DocumentKind,
                    nameof(RuntimeTarget),
                    nameof(DocumentIRNode.Target));
                throw new InvalidOperationException(message);
            }

            var codeWriter = new CSharpCodeWriter();
            var newLineString = codeDocument.Items[NewLineString];
            if (newLineString != null)
            {
                // Set new line character to a specific string regardless of platform, for testing purposes.
                codeWriter.NewLine = (string)newLineString;
            }

            var renderingContext = new CSharpRenderingContext()
            {
                Writer = codeWriter,
                SourceDocument = codeDocument.Source,
                Options = irDocument.Options,
            };

            var idValue = codeDocument.Items[SuppressUniqueIds];
            if (idValue != null)
            {
                // Generate a static value for unique ids instead of a guid, for testing purposes.
                renderingContext.IdGenerator = () => idValue.ToString();
            }

            var renderer = target.CreateRenderer(renderingContext);
            renderer.VisitDocument(irDocument);

            var diagnostics = new List<RazorDiagnostic>();

            // Temporary code while we're still using legacy diagnostics in the SyntaxTree.
            for (var i = 0; i < syntaxTree.Diagnostics.Count; i++)
            {
                diagnostics.Add(RazorDiagnostic.Create(syntaxTree.Diagnostics[i]));
            }

            for (var i = 0; i < renderingContext.ErrorSink.Errors.Count; i++)
            {
                diagnostics.Add(RazorDiagnostic.Create(renderingContext.ErrorSink.Errors[i]));
            }
            
            var csharpDocument = new RazorCSharpDocument()
            {
                GeneratedCode = renderingContext.Writer.GenerateCode(),
                LineMappings = renderingContext.LineMappings,
                Diagnostics = diagnostics,
            };

            codeDocument.SetCSharpDocument(csharpDocument);
        }
    }
}
