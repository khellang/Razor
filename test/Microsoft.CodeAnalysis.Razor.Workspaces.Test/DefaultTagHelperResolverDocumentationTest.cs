// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.Workspaces
{
    public class DefaultTagHelperResolverDocumentationTest
    {
        [Fact]
        public void DoIt()
        {
            var thr = new DefaultTagHelperResolver(designTime: true);

            var compilation = CreateCompilation(@"
namespace Test 
{
    /// <summary>
    /// This is a summary.
    /// </summary>
    public class SourceCodeTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
    {
    }
}");
            var results = thr.GetTagHelpers(compilation);
            var tagHelper = Assert.Single(results, t => t.TypeName == "Test.SourceCodeTagHelper");
            Assert.Equal("This is a summary.", tagHelper.DesignTimeDescriptor.Summary);

        }


        private Compilation CreateCompilation(string text = null)
        {
            var syntaxTrees = new List<SyntaxTree>();
            if (text != null)
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(text));
            }

            return CSharpCompilation.Create("TestAssembly", syntaxTrees: syntaxTrees, references: new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ITagHelper).Assembly.Location),
                MetadataReference.CreateFromFile(GetType().Assembly.Location),
            });
        }
    }

    /// <summary>
    /// This is a summary.
    /// </summary>
    public class MyTagHelper : TagHelper
    {
    }
}
