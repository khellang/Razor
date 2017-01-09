// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution.ApiSets
{
    public sealed class CSharpWritingContext
    {
        private readonly ApiSetIRNodeWalker _walker;

        public CSharpWritingContext(ApiSet apiSet, RazorCodeDocument codeDocument, RazorParserOptions options)
        {
            if (apiSet == null)
            {
                throw new ArgumentNullException(nameof(apiSet));
            }

            if (codeDocument == null)
            {
                throw new ArgumentNullException(nameof(codeDocument));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            ApiSet = apiSet;
            CodeDocument = codeDocument;
            Options = options;

            Writer = new CSharpCodeWriter();
            _walker = new ApiSetIRNodeWalker(apiSet, this);
        }

        public ApiSet ApiSet { get; }

        public RazorCodeDocument CodeDocument { get; }

        public RazorParserOptions Options { get; }

        public CSharpCodeWriter Writer { get; }

        public ErrorSink Errors { get; } = new ErrorSink();

        public Func<string> IdGenerator { get; set; } = () => Guid.NewGuid().ToString("N");

        public string CurrentWriter { get; set; }

        public IList<LineMapping> LineMappings { get; } = new List<LineMapping>();

        public void WriteChildren(RazorIRNode node)
        {
            for (var i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                _walker.Visit(child);
            }
        }
    }
}
