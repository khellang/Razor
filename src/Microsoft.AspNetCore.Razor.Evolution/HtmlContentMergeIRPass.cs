// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    internal class HtmlContentMergeIRPass : RazorIRPassBase
    {
        public override int Order => RazorIRPass.DefaultLoweringOrder;

        public override DocumentIRNode ExecuteCore(RazorCodeDocument codeDocument, DocumentIRNode irDocument)
        {
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            foreach (var node in visitor.NodesWithHtml)
            {
                MergeConsecutiveHtmlNodes(node);
            }

            return irDocument;
        }

        private void MergeConsecutiveHtmlNodes(RazorIRNode node)
        {
            for (var i = node.Children.Count - 1; i >= 1; i--)
            {
                var node1 = node.Children[i - 1] as HtmlContentIRNode;
                var node2 = node.Children[i] as HtmlContentIRNode;

                if (node1 != null &&
                    node2 != null &&
                    node1.SourceRange != null &&
                    node2.SourceRange != null &&
                    node1.SourceRange.FilePath == node2.SourceRange.FilePath &&
                    node1.SourceRange.AbsoluteIndex + node1.Content.Length == node2.SourceRange.AbsoluteIndex)
                {
                    node1.Content = node1.Content + node2.Content;
                    node1.SourceRange = new MappingLocation(
                        node1.SourceRange.AbsoluteIndex,
                        node1.SourceRange.LineIndex,
                        node1.SourceRange.CharacterIndex,
                        node1.SourceRange.ContentLength + node2.Content.Length,
                        node1.SourceRange.FilePath);

                    node.Children.RemoveAt(i);
                }
            }
        }

        // Tag nodes containing HTML so we can rewrite them.
        private class Visitor : RazorIRNodeWalker
        {
            public HashSet<RazorIRNode> NodesWithHtml { get; } = new HashSet<RazorIRNode>();

            public override void VisitHtml(HtmlContentIRNode node)
            {
                if (node.Parent != null && node.Parent.Children.Count > 1)
                {
                    NodesWithHtml.Add(node.Parent);
                }
            }
        }
    }
}
