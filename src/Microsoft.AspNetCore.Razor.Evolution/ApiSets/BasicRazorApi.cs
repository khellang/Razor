// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution.ApiSets
{
    public class BasicRazorApi : IBasicRazorApi
    {
        public string WriteMethodName { get; set; } = "Write";

        public string WriteRedirectedMethodName { get; set; } = "WriteTo";

        public string WriteLiteralMethodName { get; set; } = "WriteLiteral";

        public string WriteLiteralRedirctedMethodName { get; set; } = "WriteLiteralTo";

        public string BeginWriteAttributeMethodName { get; set; } = "BeginWriteAttribute";

        public string BeginWriteAttributeRedirctedMethodName { get; set; } = "BeginWriteAttributeTo";

        public string WriteAttributeValueMethodName { get; set; } = "WriteAttributeValue";

        public string WriteAttributeValueRedirctedMethodName { get; set; } = "WriteAttributeValueTo";

        public string EndWriteAttributeMethodName { get; set; } = "EndWriteAttribute";

        public string EndWriteAttributeRedirctedMethodName { get; set; } = "EndWriteAttributeTo";

        public void WriteChecksum(CSharpWritingContext context, ChecksumIRNode node)
        {
            if (context.Options.DesignTimeMode)
            {
                return;
            }

            if (!string.IsNullOrEmpty(node.Bytes))
            {
                context.Writer
                    .Write("#pragma checksum \"")
                    .Write(node.Filename)
                    .Write("\" \"")
                    .Write(node.Guid)
                    .Write("\" \"")
                    .Write(node.Bytes)
                    .WriteLine("\"");
            }
        }

        public void WriteClass(CSharpWritingContext context, ClassDeclarationIRNode node)
        {
            context.Writer
                .Write(node.AccessModifier)
                .Write(" class ")
                .Write(node.Name);

            if (node.BaseType != null || node.Interfaces != null)
            {
                context.Writer.Write(" : ");
            }

            if (node.BaseType != null)
            {
                context.Writer.Write(node.BaseType);

                if (node.Interfaces != null)
                {
                    context.Writer.WriteParameterSeparator();
                }
            }

            if (node.Interfaces != null)
            {
                for (var i = 0; i < node.Interfaces.Count; i++)
                {
                    context.Writer.Write(node.Interfaces[i]);

                    if (i + 1 < node.Interfaces.Count)
                    {
                        context.Writer.WriteParameterSeparator();
                    }
                }
            }

            context.Writer.WriteLine();

            using (context.Writer.BuildScope())
            {
                context.WriteChildren(node);
            }
        }

        public void WriteCSharpAttributeValue(CSharpWritingContext context, CSharpAttributeValueIRNode node)
        {
            const string ValueWriterName = "__razor_attribute_value_writer";

            var expressionValue = node.Children.FirstOrDefault() as CSharpExpressionIRNode;

            if (expressionValue != null && expressionValue.Source != null)
            {
                context.Writer.WriteLineNumberDirective(node.Source.Value);
            }

            StartBeginWriteAttributeMethod(context);

            var prefixLocation = node.Source.Value.AbsoluteIndex;
            var valueLocation = node.Source.Value.AbsoluteIndex + node.Prefix.Length;
            var valueLength = node.Source.Value.Length - node.Prefix.Length;
            context.Writer
                .WriteStringLiteral(node.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator();

            if (expressionValue != null)
            {
                Debug.Assert(node.Children.Count == 1);

                WriteCSharpTokens(context, expressionValue);
            }
            else
            {
                // Not an expression; need to buffer the result.
                context.Writer.WriteStartNewObject("Microsoft.AspNetCore.Mvc.Razor.HelperResult");

                var textWriter = context.CurrentWriter;
                context.CurrentWriter = ValueWriterName;

                using (context.Writer.BuildAsyncLambda(endLine: false, parameterNames: ValueWriterName))
                {
                    context.WriteChildren(node);
                }

                context.CurrentWriter = textWriter;
                
                context.Writer.WriteEndMethodInvocation(false);
            }

            context.Writer
                .WriteParameterSeparator()
                .Write(valueLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valueLength.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteBooleanLiteral(false)
                .WriteEndMethodInvocation();

            if (expressionValue != null && expressionValue.Source != null)
            {
                context.Writer.WriteEndLineNumberDirective();
            }
        }

        public void WriteCSharpExpression(CSharpWritingContext context, CSharpExpressionIRNode node)
        {
            if (context.Options.DesignTimeMode)
            {
                if (node.Children.Count == 0)
                {
                    return;
                }

                if (node.Source != null)
                {
                    context.Writer.WriteLineNumberDirective(node.Source.Value);

                    var padding = BuildOffsetPadding(context, node.Source.Value, RazorDesignTimeIRPass.DesignTimeVariable.Length);

                    context.Writer
                        .Write(padding)
                        .WriteStartAssignment(RazorDesignTimeIRPass.DesignTimeVariable);

                    for (var i = 0; i < node.Children.Count; i++)
                    {
                        var childNode = node.Children[i] as CSharpTokenIRNode;
                        if (childNode != null)
                        {
                            AddLineMappingFor(context, childNode);
                            context.Writer.Write(childNode.Content);
                        }
                    }

                    context.Writer.WriteLine(";");

                    context.Writer.WriteEndLineNumberDirective();
                }
                else
                {
                    context.Writer.WriteStartAssignment(RazorDesignTimeIRPass.DesignTimeVariable);

                    for (var i = 0; i < node.Children.Count; i++)
                    {
                        var childNode = node.Children[i] as CSharpTokenIRNode;
                        if (childNode != null)
                        {
                            AddLineMappingFor(context, childNode);
                            context.Writer.Write(childNode.Content);
                        }
                    }

                    context.Writer.WriteLine(";");
                }
            }
            else
            {
                if (node.Source != null)
                {
                    context.Writer.WriteLineNumberDirective(node.Source.Value);
                }

                StartWriteMethod(context, node);

                WriteCSharpTokens(context, node);

                context.Writer.WriteEndMethodInvocation();

                if (node.Source != null)
                {
                    context.Writer.WriteEndLineNumberDirective();
                }
            }
        }

        public void WriteCSharpStatement(CSharpWritingContext context, CSharpStatementIRNode node)
        {
            if (string.IsNullOrWhiteSpace(node.Content))
            {
                return;
            }

            if (node.Source != null)
            {

                context.Writer.WriteLineNumberDirective(node.Source.Value);

                var padding = BuildOffsetPadding(context, node.Source, 0);
                context.Writer.Write(padding);

                AddLineMappingFor(context, node);
                context.Writer.WriteLine(node.Content);

                context.Writer.WriteEndLineNumberDirective();
            }
            else
            {
                context.Writer.WriteLine(node.Content);
            }
        }

        public void WriteDirective(CSharpWritingContext context, DirectiveIRNode node)
        {
            if (!context.Options.DesignTimeMode)
            {
                return;
            }

            const string TypeHelper = "__typeHelper";

            for (var i = 0; i < node.Children.Count; i++)
            {
                var token = node.Children[i] as DirectiveTokenIRNode;
                if (token == null)
                {
                    continue;
                }

                var tokenKind = token.Descriptor.Kind;
                if (token.Source == null)
                {
                    return;
                }

                // Wrap the directive token in a lambda to isolate variable names.
                context.Writer
                    .Write("((")
                    .Write(typeof(Action).FullName)
                    .Write(")(");
                using (context.Writer.BuildLambda(endLine: false))
                {
                    var originalIndent = context.Writer.CurrentIndent;
                    context.Writer.ResetIndent();
                    switch (tokenKind)
                    {
                        case DirectiveTokenKind.Type:

                            AddLineMappingFor(context, token);
                            context.Writer
                                .Write(token.Content)
                                .Write(" ")
                                .WriteStartAssignment(TypeHelper)
                                .WriteLine("null;");
                            break;
                        case DirectiveTokenKind.Member:
                            context.Writer
                                .Write(typeof(object).FullName)
                                .Write(" ");

                            AddLineMappingFor(context, token);
                            context.Writer
                                .Write(token.Content)
                                .WriteLine(" = null;");
                            break;
                        case DirectiveTokenKind.String:
                            context.Writer
                                .Write(typeof(object).FullName)
                                .Write(" ")
                                .WriteStartAssignment(TypeHelper);

                            if (token.Content.StartsWith("\"", StringComparison.Ordinal))
                            {
                                AddLineMappingFor(context, token);
                                context.Writer.Write(token.Content);
                            }
                            else
                            {
                                context.Writer.Write("\"");
                                AddLineMappingFor(context, token);
                                context.Writer
                                    .Write(token.Content)
                                    .Write("\"");
                            }

                            context.Writer.WriteLine(";");
                            break;
                    }
                    context.Writer.SetIndent(originalIndent);
                }
                context.Writer.WriteLine("))();");
            }
        }

        public void WriteDocument(CSharpWritingContext context, DocumentIRNode node)
        {
            context.WriteChildren(node);
        }

        public void WriteHtml(CSharpWritingContext context, HtmlContentIRNode node)
        {
            const int MaxStringLiteralLength = 1024;

            var charactersConsumed = 0;

            // Render the string in pieces to avoid Roslyn OOM exceptions at compile time: https://github.com/aspnet/External/issues/54
            while (charactersConsumed < node.Content.Length)
            {
                string textToRender;
                if (node.Content.Length <= MaxStringLiteralLength)
                {
                    textToRender = node.Content;
                }
                else
                {
                    var charactersToSubstring = Math.Min(MaxStringLiteralLength, node.Content.Length - charactersConsumed);
                    textToRender = node.Content.Substring(charactersConsumed, charactersToSubstring);
                }

                StartWriteLiteralMethod(context);
                context.Writer.WriteStringLiteral(textToRender);
                context.Writer.WriteEndMethodInvocation();

                charactersConsumed += textToRender.Length;
            }
        }

        public void WriteHtmlAttribute(CSharpWritingContext context, HtmlAttributeIRNode node)
        {
            StartBeginWriteAttributeMethod(context);

            var valuePieceCount = node
                .Children
                .Count(child => child is HtmlAttributeValueIRNode || child is CSharpAttributeValueIRNode);
            var prefixLocation = node.Source.Value.AbsoluteIndex;
            var suffixLocation = node.Source.Value.AbsoluteIndex + node.Source.Value.Length - node.Suffix.Length;
            context.Writer
                .WriteStringLiteral(node.Name)
                .WriteParameterSeparator()
                .WriteStringLiteral(node.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteStringLiteral(node.Suffix)
                .WriteParameterSeparator()
                .Write(suffixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valuePieceCount.ToString(CultureInfo.InvariantCulture))
                .WriteEndMethodInvocation();

            context.WriteChildren(node);

            WriteEndAttributeMethod(context);
        }

        public void WriteHtmlAttributeValue(CSharpWritingContext context, HtmlAttributeValueIRNode node)
        {
            StartWriteAttributeValueMethod(context);

            var prefixLocation = node.Source.Value.AbsoluteIndex;
            var valueLocation = node.Source.Value.AbsoluteIndex + node.Prefix.Length;
            var valueLength = node.Source.Value.Length;
            context.Writer
                .WriteStringLiteral(node.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteStringLiteral(node.Content)
                .WriteParameterSeparator()
                .Write(valueLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valueLength.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteBooleanLiteral(true)
                .WriteEndMethodInvocation();
        }

        public void WriteNamespace(CSharpWritingContext context, NamespaceDeclarationIRNode node)
        {
            context.Writer
                .Write("namespace ")
                .WriteLine(node.Content);

            using (context.Writer.BuildScope())
            {
                context.Writer.WriteLineHiddenDirective();
                context.WriteChildren(node);
            }
        }

        public void WriteMethod(CSharpWritingContext context, RazorMethodDeclarationIRNode node)
        {
            context.Writer.WriteLine("#pragma warning disable 1998");

            context.Writer
                .Write(node.AccessModifier)
                .Write(" ");

            if (node.Modifiers != null)
            {
                for (var i = 0; i < node.Modifiers.Count; i++)
                {
                    context.Writer.Write(node.Modifiers[i]);

                    if (i + 1 < node.Modifiers.Count)
                    {
                        context.Writer.Write(" ");
                    }
                }
            }

            context.Writer
                .Write(" ")
                .Write(node.ReturnType)
                .Write(" ")
                .Write(node.Name)
                .WriteLine("()");

            using (context.Writer.BuildScope())
            {
                context.WriteChildren(node);
            }

            context.Writer.WriteLine("#pragma warning restore 1998");
        }

        public void WriteUsing(CSharpWritingContext context, UsingStatementIRNode node)
        {
            context.Writer.WriteUsing(node.Content);
        }

        protected void WriteCSharpTokens(CSharpWritingContext context, RazorIRNode node)
        {
            for (var i = 0; i < node.Children.Count; i++)
            {
                var token = node.Children[i] as CSharpTokenIRNode;
                if (token != null)
                {
                    context.Writer.Write(token.Content);
                }
            }
        }

        private static void AddLineMappingFor(CSharpWritingContext context, RazorIRNode node)
        {
            if (!context.Options.DesignTimeMode)
            {
                return;
            }

            var sourceLocation = node.Source.Value;
            var generatedLocation = new SourceSpan(context.Writer.GetCurrentSourceLocation(), sourceLocation.Length);
            var lineMapping = new LineMapping(sourceLocation, generatedLocation);

            context.LineMappings.Add(lineMapping);
        }

        protected static string BuildOffsetPadding(CSharpWritingContext context, SourceSpan? span, int offset)
        {
            if (span == null)
            {
                return string.Empty;
            }

            var basePadding = CalculateExpressionPadding(context, span.Value);
            var resolvedPadding = Math.Max(basePadding - offset, 0);

            if (context.Options.IsIndentingWithTabs)
            {
                var spaces = resolvedPadding % context.Options.TabSize;
                var tabs = resolvedPadding / context.Options.TabSize;

                return new string('\t', tabs) + new string(' ', spaces);
            }
            else
            {
                return new string(' ', resolvedPadding);
            }
        }

        protected static int CalculateExpressionPadding(CSharpWritingContext context, SourceSpan sourceRange)
        {
            var spaceCount = 0;
            for (var i = sourceRange.AbsoluteIndex - 1; i >= 0; i--)
            {
                var @char = context.CodeDocument.Source[i];
                if (@char == '\n' || @char == '\r')
                {
                    break;
                }
                else if (@char == '\t')
                {
                    spaceCount += context.Options.TabSize;
                }
                else
                {
                    spaceCount++;
                }
            }

            return spaceCount;
        }

        private void StartWriteMethod(CSharpWritingContext context, RazorIRNode node)
        {
            if (context.CurrentWriter == null)
            {
                var offset = WriteMethodName.Length + 1;
                var padding = BuildOffsetPadding(context, node.Source, offset);
                context.Writer.Write(padding);

                context.Writer.WriteStartMethodInvocation(WriteMethodName);
            }
            else
            {
                var offset = WriteRedirectedMethodName.Length + 1 + context.CurrentWriter.Length + 2;
                var padding = BuildOffsetPadding(context, node.Source, offset);
                context.Writer.Write(padding);

                context.Writer.WriteStartMethodInvocation(WriteRedirectedMethodName);
                context.Writer.Write(context.CurrentWriter);
                context.Writer.WriteParameterSeparator();
            }
        }

        private void StartWriteLiteralMethod(CSharpWritingContext context)
        {
            if (context.CurrentWriter == null)
            {
                context.Writer.WriteStartMethodInvocation(WriteLiteralMethodName);
            }
            else
            {
                context.Writer.WriteStartMethodInvocation(WriteLiteralRedirctedMethodName);
                context.Writer.Write(context.CurrentWriter);
                context.Writer.WriteParameterSeparator();
            }
        }

        private void StartBeginWriteAttributeMethod(CSharpWritingContext context)
        {
            if (context.CurrentWriter == null)
            {
                context.Writer.WriteStartMethodInvocation(BeginWriteAttributeMethodName);
            }
            else
            {
                context.Writer.WriteStartMethodInvocation(BeginWriteAttributeRedirctedMethodName);
                context.Writer.Write(context.CurrentWriter);
                context.Writer.WriteParameterSeparator();
            }
        }

        private void StartWriteAttributeValueMethod(CSharpWritingContext context)
        {
            if (context.CurrentWriter == null)
            {
                context.Writer.WriteStartMethodInvocation(WriteAttributeValueMethodName);
            }
            else
            {
                context.Writer.WriteStartMethodInvocation(WriteAttributeValueRedirctedMethodName);
                context.Writer.Write(context.CurrentWriter);
                context.Writer.WriteParameterSeparator();
            }
        }

        private void WriteEndAttributeMethod(CSharpWritingContext context)
        {
            if (context.CurrentWriter == null)
            {
                context.Writer.WriteMethodInvocation(EndWriteAttributeMethodName);
            }
            else
            {
                context.Writer.WriteStartMethodInvocation(EndWriteAttributeRedirctedMethodName);
                context.Writer.Write(context.CurrentWriter);
                context.Writer.WriteEndMethodInvocation();
            }
        }
    }
}
