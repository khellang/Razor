// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;
using System.Globalization;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;

namespace Microsoft.AspNetCore.Razor.Evolution.ApiSets
{
    public class TagHelperApiExtension : ITagHelperApiExtension
    {
        protected TagHelperContext CurrentTagHelper { get; set; }

        public void WriteTagHelper(CSharpWritingContext context, TagHelperIRNode node)
        {
            var tagHelperContext = CurrentTagHelper;
            CurrentTagHelper = new TagHelperContext();

            context.WriteChildren(node);

            CurrentTagHelper = tagHelperContext;
        }

        public void WriteAddPreallocatedTagHelperHtmlAttribute(CSharpWritingContext context, AddPreallocatedTagHelperHtmlAttributeIRNode node)
        {
            context.Writer
                .WriteStartInstanceMethodInvocation(
                    "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */,
                    "AddHtmlAttribute" /* ORIGINAL: ExecutionContextAddHtmlAttributeMethodName */)
                .Write(node.VariableName)
                .WriteEndMethodInvocation();
        }

        public void WriteAddTagHelperHtmlAttribute(CSharpWritingContext context, AddTagHelperHtmlAttributeIRNode node)
        {
            var attributeValueStyleParameter = $"global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.{node.ValueStyle}";
            var isConditionalAttributeValue = node.Children.Any(child => child is CSharpAttributeValueIRNode);

            // All simple text and minimized attributes will be pre-allocated.
            if (isConditionalAttributeValue)
            {
                // Dynamic attribute value should be run through the conditional attribute removal system. It's
                // unbound and contains C#.

                // TagHelper attribute rendering is buffered by default. We do not want to write to the current
                // writer.
                var valuePieceCount = node.Children.Count(
                    child => child is HtmlAttributeValueIRNode || child is CSharpAttributeValueIRNode);

                context.Writer
                    .WriteStartMethodInvocation("BeginAddHtmlAttributeValues" /* ORIGINAL: BeginAddHtmlAttributeValuesMethodName */)
                    .Write("__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */)
                    .WriteParameterSeparator()
                    .WriteStringLiteral(node.Name)
                    .WriteParameterSeparator()
                    .Write(valuePieceCount.ToString(CultureInfo.InvariantCulture))
                    .WriteParameterSeparator()
                    .Write(attributeValueStyleParameter)
                    .WriteEndMethodInvocation();

                for (var i = 0; i < node.Children.Count; i++)
                {
                    // Write each child - AddHtmlAttributeValue
                    var child = node.Children[i] as HtmlAttributeValueIRNode;
                }

                context.Writer
                    .WriteMethodInvocation(
                        "EndAddHtmlAttributeValues" /* ORIGINAL: EndAddHtmlAttributeValuesMethodName */,
                        "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */);
            }
            else
            {
                // This is a data-* attribute which includes C#. Do not perform the conditional attribute removal or
                // other special cases used when IsDynamicAttributeValue(). But the attribute must still be buffered to
                // determine its final value.

                // Attribute value is not plain text, must be buffered to determine its final value.
                context.Writer.WriteMethodInvocation("BeginWriteTagHelperAttribute" /* ORIGINAL: BeginWriteTagHelperAttributeMethodName */);

                // We're building a writing scope around the provided chunks which captures everything written from the
                // page. Therefore, we do not want to write to any other buffer since we're using the pages buffer to
                // ensure we capture all content that's written, directly or indirectly.
                var textWriter = context.CurrentWriter;
                context.CurrentWriter = null;

                context.WriteChildren(node);

                context.CurrentWriter = textWriter;

                context.Writer
                    .WriteStartAssignment("__tagHelperStringValueBuffer" /* ORIGINAL: StringValueBufferVariableName */)
                    .WriteMethodInvocation("EndWriteTagHelperAttribute" /* ORIGINAL: EndWriteTagHelperAttributeMethodName */)
                    .WriteStartInstanceMethodInvocation(
                        "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */,
                        "AddHtmlAttribute" /* ORIGINAL: ExecutionContextAddHtmlAttributeMethodName */)
                    .WriteStringLiteral(node.Name)
                    .WriteParameterSeparator()
                    .WriteStartMethodInvocation("Html.Raw" /* ORIGINAL: MarkAsHtmlEncodedMethodName */)
                    .Write("__tagHelperStringValueBuffer" /* ORIGINAL: StringValueBufferVariableName */)
                    .WriteEndMethodInvocation(endLine: false)
                    .WriteParameterSeparator()
                    .Write(attributeValueStyleParameter)
                    .WriteEndMethodInvocation();
            }
        }

        public void WriteCreateTagHelper(CSharpWritingContext context, CreateTagHelperIRNode node)
        {
            var tagHelperVariableName = GetTagHelperVariableName(node.TagHelperTypeName);

            context.Writer
                .WriteStartAssignment(tagHelperVariableName)
                .WriteStartMethodInvocation(
                    "CreateTagHelper" /* ORIGINAL: CreateTagHelperMethodName */,
                    "global::" + node.TagHelperTypeName)
                .WriteEndMethodInvocation();

            if (!context.Options.DesignTimeMode)
            {
                context.Writer.WriteInstanceMethodInvocation(
                    "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */,
                    "Add" /* ORIGINAL: ExecutionContextAddMethodName */,
                    tagHelperVariableName);
            }
        }

        public void WriteDeclarePreallocatedTagHelperAttribute(CSharpWritingContext context, DeclarePreallocatedTagHelperAttributeIRNode node)
        {
            context.Writer
                .Write("private static readonly global::")
                .Write("Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute" /* ORIGINAL: TagHelperAttributeTypeName */)
                .Write(" ")
                .Write(node.VariableName)
                .Write(" = ")
                .WriteStartNewObject("global::" + "Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute" /* ORIGINAL: TagHelperAttributeTypeName */)
                .WriteStringLiteral(node.Name)
                .WriteParameterSeparator()
                .WriteStringLiteral(node.Value)
                .WriteParameterSeparator()
                .Write($"global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.{node.ValueStyle}")
                .WriteEndMethodInvocation();
        }

        public void WriteDeclarePreallocatedTagHelperHtmlAttribute(CSharpWritingContext context, DeclarePreallocatedTagHelperHtmlAttributeIRNode node)
        {
            context.Writer
                .Write("private static readonly global::")
                .Write("Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute" /* ORIGINAL: TagHelperAttributeTypeName */)
                .Write(" ")
                .Write(node.VariableName)
                .Write(" = ")
                .WriteStartNewObject("global::" + "Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute" /* ORIGINAL: TagHelperAttributeTypeName */)
                .WriteStringLiteral(node.Name);

            if (node.ValueStyle == HtmlAttributeValueStyle.Minimized)
            {
                context.Writer.WriteEndMethodInvocation();
            }
            else
            {
                context.Writer
                    .WriteParameterSeparator()
                    .WriteStartNewObject("global::" + "Microsoft.AspNetCore.Html.HtmlString" /* ORIGINAL: EncodedHtmlStringTypeName */)
                    .WriteStringLiteral(node.Value)
                    .WriteEndMethodInvocation(endLine: false)
                    .WriteParameterSeparator()
                    .Write($"global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.{node.ValueStyle}")
                    .WriteEndMethodInvocation();
            }
        }

        public void WriteDeclareTagHelperFields(CSharpWritingContext context, DeclareTagHelperFieldsIRNode node)
        {
            if (!context.Options.DesignTimeMode)
            {
                context.Writer.WriteLineHiddenDirective();

                // Need to disable the warning "X is assigned to but never used." for the value buffer since
                // whether it's used depends on how a TagHelper is used.
                context.Writer
                    .WritePragma("warning disable 0414")
                    .Write("private ")
                    .WriteVariableDeclaration("string", "__tagHelperStringValueBuffer" /* ORIGINAL: StringValueBufferVariableName */, value: null)
                    .WritePragma("warning restore 0414");

                context.Writer
                    .Write("private global::")
                    .WriteVariableDeclaration(
                        "Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext" /* ORIGINAL: ExecutionContextTypeName */,
                        "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */,
                        value: null);

                context.Writer
                    .Write("private global::")
                    .Write("Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner" /* ORIGINAL: RunnerTypeName */)
                    .Write(" ")
                    .Write("__tagHelperRunner" /* ORIGINAL: RunnerVariableName */)
                    .Write(" = new global::")
                    .Write("Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner" /* ORIGINAL: RunnerTypeName */)
                    .WriteLine("();");

                const string backedScopeManageVariableName = "__backed" + "__tagHelperScopeManager" /* ORIGINAL: ScopeManagerVariableName */;
                context.Writer
                    .Write("private global::")
                    .WriteVariableDeclaration(
                        "Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager",
                        backedScopeManageVariableName,
                        value: null);

                context.Writer
                    .Write("private global::")
                    .Write("Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager" /* ORIGINAL: ScopeManagerTypeName */)
                    .Write(" ")
                    .WriteLine("__tagHelperScopeManager" /* ORIGINAL: ScopeManagerVariableName */);

                using (context.Writer.BuildScope())
                {
                    context.Writer.WriteLine("get");
                    using (context.Writer.BuildScope())
                    {
                        context.Writer
                            .Write("if (")
                            .Write(backedScopeManageVariableName)
                            .WriteLine(" == null)");

                        using (context.Writer.BuildScope())
                        {
                            context.Writer
                                .WriteStartAssignment(backedScopeManageVariableName)
                                .WriteStartNewObject("Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager" /* ORIGINAL: ScopeManagerTypeName */)
                                .Write("StartTagHelperWritingScope" /* ORIGINAL: StartTagHelperWritingScopeMethodName */)
                                .WriteParameterSeparator()
                                .Write("EndTagHelperWritingScope" /* ORIGINAL: EndTagHelperWritingScopeMethodName */)
                                .WriteEndMethodInvocation();
                        }

                        context.Writer.WriteReturn(backedScopeManageVariableName);
                    }
                }
            }

            foreach (var tagHelperTypeName in node.UsedTagHelperTypeNames)
            {
                var tagHelperVariableName = GetTagHelperVariableName(tagHelperTypeName);
                context.Writer
                    .Write("private global::")
                    .WriteVariableDeclaration(
                        tagHelperTypeName,
                        tagHelperVariableName,
                        value: null);
            }
        }

        public void WriteExecuteTagHelpers(CSharpWritingContext context, ExecuteTagHelpersIRNode node)
        {
            context.Writer
                .Write("await ")
                .WriteStartInstanceMethodInvocation(
                    "__tagHelperRunner" /* ORIGINAL: RunnerVariableName */,
                    "RunAsync" /* ORIGINAL: RunnerRunAsyncMethodName */)
                .Write("__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */)
                .WriteEndMethodInvocation();

            var executionContextVariableName = "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */;
            var executionContextOutputPropertyName = "Output" /* ORIGINAL: ExecutionContextOutputPropertyName */;
            var tagHelperOutputAccessor = $"{executionContextVariableName}.{executionContextOutputPropertyName}";

            context.Writer
                .Write("if (!")
                .Write(tagHelperOutputAccessor)
                .Write(".")
                .Write("IsContentModified" /* ORIGINAL: TagHelperOutputIsContentModifiedPropertyName */)
                .WriteLine(")");

            using (context.Writer.BuildScope())
            {
                context.Writer
                    .Write("await ")
                    .WriteInstanceMethodInvocation(
                        executionContextVariableName,
                        "SetOutputContentAsync" /* ORIGINAL: ExecutionContextSetOutputContentAsyncMethodName */);
            }

            // EVIL
            context.Writer
                .Write("Write")
                .Write(tagHelperOutputAccessor)
                .WriteEndMethodInvocation()
                .WriteStartAssignment(executionContextVariableName)
                .WriteInstanceMethodInvocation(
                    "__tagHelperScopeManager" /* ORIGINAL: ScopeManagerVariableName */,
                    "End" /* ORIGINAL: ScopeManagerEndMethodName */);
        }

        public void WriteInitializeTagHelperStructure(CSharpWritingContext context, InitializeTagHelperStructureIRNode node)
        {
            if (context.Options.DesignTimeMode)
            {
                return;
            }

            // Call into the tag helper scope manager to start a new tag helper scope.
            // Also capture the value as the current execution context.
            context.Writer
                .WriteStartAssignment("__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */)
                .WriteStartInstanceMethodInvocation(
                    "__tagHelperScopeManager" /* ORIGINAL: ScopeManagerVariableName */,
                    "Begin" /* ORIGINAL: ScopeManagerBeginMethodName */);

            // Assign a unique ID for this instance of the source HTML tag. This must be unique
            // per call site, e.g. if the tag is on the view twice, there should be two IDs.
            context.Writer.WriteStringLiteral(node.TagName)
                .WriteParameterSeparator()
                .Write("global::")
                .Write("Microsoft.AspNetCore.Razor.TagHelpers.TagMode")
                .Write(".")
                .Write(node.TagMode.ToString())
                .WriteParameterSeparator()
                .WriteStringLiteral(context.IdGenerator())
                .WriteParameterSeparator();

            // We remove and redirect writers so TagHelper authors can retrieve content.
            var textWriter = context.CurrentWriter;
            context.CurrentWriter = null;

            using (context.Writer.BuildAsyncLambda(endLine: false))
            {
                context.WriteChildren(node);
            }

            context.CurrentWriter = textWriter;

            context.Writer.WriteEndMethodInvocation();
        }

        public void WriteSetPreallocatedTagHelperProperty(CSharpWritingContext context, SetPreallocatedTagHelperPropertyIRNode node)
        {
            var tagHelperVariableName = GetTagHelperVariableName(node.TagHelperTypeName);
            var propertyValueAccessor = GetTagHelperPropertyAccessor(tagHelperVariableName, node.AttributeName, node.Descriptor);
            var attributeValueAccessor = $"{node.VariableName}.Value" /* ORIGINAL: TagHelperAttributeValuePropertyName */;
            context.Writer
                .WriteStartAssignment(propertyValueAccessor)
                .Write("(string)")
                .Write(attributeValueAccessor)
                .WriteLine(";")
                .WriteStartInstanceMethodInvocation(
                    "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */,
                    "AddTagHelperAttribute" /* ORIGINAL: ExecutionContextAddTagHelperAttributeMethodName */)
                .Write(node.VariableName)
                .WriteEndMethodInvocation();
        }

        public void WriteSetTagHelperProperty(CSharpWritingContext context, SetTagHelperPropertyIRNode node)
        {
            if (context.Options.DesignTimeMode)
            {
                var tagHelperVariableName = GetTagHelperVariableName(node.TagHelperTypeName);
                var propertyValueAccessor = GetTagHelperPropertyAccessor(tagHelperVariableName, node.AttributeName, node.Descriptor);

                string previousValueAccessor;
                if (CurrentTagHelper.RenderedBoundAttributes.TryGetValue(node.AttributeName, out previousValueAccessor))
                {
                    context.Writer
                        .WriteStartAssignment(propertyValueAccessor)
                        .Write(previousValueAccessor)
                        .WriteLine(";");

                    return;
                }
                else
                {
                    CurrentTagHelper.RenderedBoundAttributes[node.AttributeName] = propertyValueAccessor;
                }

                if (node.Descriptor.IsStringProperty)
                {
                    context.Writer.WriteStartAssignment(propertyValueAccessor);
                    if (node.Children.Count == 1 && node.Children.First() is HtmlContentIRNode)
                    {
                        var htmlNode = node.Children.First() as HtmlContentIRNode;
                        if (htmlNode != null)
                        {
                            context.Writer.WriteStringLiteral(htmlNode.Content);
                        }
                    }
                    else
                    {
                        context.Writer.Write("string.Empty");
                    }
                    context.Writer.WriteLine(";");
                }
                else
                {
                    var firstMappedChild = node.Children.FirstOrDefault(child => child.Source != null) as RazorIRNode;
                    var valueStart = firstMappedChild?.Source;

                    context.Writer.WriteLineNumberDirective(node.Source.Value);

                    var assignmentPrefixLength = propertyValueAccessor.Length + " = ".Length;
                    if (node.Descriptor.IsEnum &&
                        node.Children.Count == 1 &&
                        node.Children.First() is HtmlContentIRNode)
                    {
                        assignmentPrefixLength += $"global::{node.Descriptor.TypeName}.".Length;

                        if (valueStart != null)
                        {
                            var padding = BuildOffsetPadding(context, node.Source, assignmentPrefixLength);

                            context.Writer.Write(padding);
                        }

                        context.Writer
                            .WriteStartAssignment(propertyValueAccessor)
                            .Write("global::")
                            .Write(node.Descriptor.TypeName)
                            .Write(".");
                    }
                    else
                    {
                        if (valueStart != null)
                        {
                            var padding = BuildOffsetPadding(context, node.Source, assignmentPrefixLength);

                            context.Writer.Write(padding);
                        }

                        context.Writer.WriteStartAssignment(propertyValueAccessor);
                    }

                    RenderTagHelperAttributeInline(context, node, node.Source.Value);

                    context.Writer.WriteLine(";");

                    context.Writer.WriteEndLineNumberDirective();
                }
            }
            else
            {
                var tagHelperVariableName = GetTagHelperVariableName(node.TagHelperTypeName);

                // Ensure that the property we're trying to set has initialized its dictionary bound properties.
                if (node.Descriptor.IsIndexer &&
                    CurrentTagHelper.VerifiedPropertyDictionaries.Add(node.Descriptor.PropertyName))
                {
                    // Throw a reasonable Exception at runtime if the dictionary property is null.
                    context.Writer
                        .Write("if (")
                        .Write(tagHelperVariableName)
                        .Write(".")
                        .Write(node.Descriptor.PropertyName)
                        .WriteLine(" == null)");
                    using (context.Writer.BuildScope())
                    {
                        // System is in Host.NamespaceImports for all MVC scenarios. No need to generate FullName
                        // of InvalidOperationException type.
                        context.Writer
                            .Write("throw ")
                            .WriteStartNewObject(nameof(InvalidOperationException))
                            .WriteStartMethodInvocation("InvalidTagHelperIndexerAssignment" /* ORIGINAL: FormatInvalidIndexerAssignmentMethodName */)
                            .WriteStringLiteral(node.AttributeName)
                            .WriteParameterSeparator()
                            .WriteStringLiteral(node.TagHelperTypeName)
                            .WriteParameterSeparator()
                            .WriteStringLiteral(node.Descriptor.PropertyName)
                            .WriteEndMethodInvocation(endLine: false)   // End of method call
                            .WriteEndMethodInvocation();   // End of new expression / throw statement
                    }
                }

                var propertyValueAccessor = GetTagHelperPropertyAccessor(tagHelperVariableName, node.AttributeName, node.Descriptor);

                string previousValueAccessor;
                if (CurrentTagHelper.RenderedBoundAttributes.TryGetValue(node.AttributeName, out previousValueAccessor))
                {
                    context.Writer
                        .WriteStartAssignment(propertyValueAccessor)
                        .Write(previousValueAccessor)
                        .WriteLine(";");

                    return;
                }
                else
                {
                    CurrentTagHelper.RenderedBoundAttributes[node.AttributeName] = propertyValueAccessor;
                }

                if (node.Descriptor.IsStringProperty)
                {
                    context.Writer.WriteMethodInvocation("BeginWriteTagHelperAttribute" /* ORIGINAL: BeginWriteTagHelperAttributeMethodName */);


                    for (var i = 0; i < node.Children.Count; i++)
                    {
                        // Write children with WriteLiteral
                        var html = node.Children[i] as HtmlAttributeIRNode;
                        var csharp = node.Children[i] as CSharpAttributeValueIRNode;
                    }

                    context.Writer
                        .WriteStartAssignment("__tagHelperStringValueBuffer" /* ORIGINAL: StringValueBufferVariableName */)
                        .WriteMethodInvocation("EndWriteTagHelperAttribute" /* ORIGINAL: EndWriteTagHelperAttributeMethodName */)
                        .WriteStartAssignment(propertyValueAccessor)
                        .Write("__tagHelperStringValueBuffer" /* ORIGINAL: StringValueBufferVariableName */)
                        .WriteLine(";");
                }
                else
                {
                    context.Writer.WriteLineNumberDirective(node.Source.Value);

                    context.Writer.WriteStartAssignment(propertyValueAccessor);

                    if (node.Descriptor.IsEnum &&
                        node.Children.Count == 1 &&
                        node.Children.First() is HtmlContentIRNode)
                    {
                        context.Writer
                            .Write("global::")
                            .Write(node.Descriptor.TypeName)
                            .Write(".");
                    }

                    RenderTagHelperAttributeInline(context, node, node.Source.Value);

                    context.Writer.WriteLine(";");

                    context.Writer.WriteEndLineNumberDirective();
                }

                // We need to inform the context of the attribute value.
                context.Writer
                    .WriteStartInstanceMethodInvocation(
                        "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */,
                        "AddTagHelperAttribute" /* ORIGINAL: ExecutionContextAddTagHelperAttributeMethodName */)
                    .WriteStringLiteral(node.AttributeName)
                    .WriteParameterSeparator()
                    .Write(propertyValueAccessor)
                    .WriteParameterSeparator()
                    .Write($"global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.{node.ValueStyle}")
                    .WriteEndMethodInvocation();
            }
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

        protected static string GetTagHelperVariableName(string tagHelperTypeName) => "__" + tagHelperTypeName.Replace('.', '_');

        protected static string GetTagHelperPropertyAccessor(
            string tagHelperVariableName,
            string attributeName,
            TagHelperAttributeDescriptor descriptor)
        {
            var propertyAccessor = $"{tagHelperVariableName}.{descriptor.PropertyName}";

            if (descriptor.IsIndexer)
            {
                var dictionaryKey = attributeName.Substring(descriptor.Name.Length);
                propertyAccessor += $"[\"{dictionaryKey}\"]";
            }

            return propertyAccessor;
        }

        private void RenderTagHelperAttributeInline(
            CSharpWritingContext context,
            RazorIRNode node,
            SourceSpan documentLocation)
        {
            if (node is SetTagHelperPropertyIRNode || node is CSharpExpressionIRNode)
            {
                for (var i = 0; i < node.Children.Count; i++)
                {
                    RenderTagHelperAttributeInline(context, node.Children[i], documentLocation);
                }
            }
            else if (node is HtmlContentIRNode)
            {
                context.Writer.Write(((HtmlContentIRNode)node).Content);
            }
            else if (node is CSharpTokenIRNode)
            {
                context.Writer.Write(((CSharpTokenIRNode)node).Content);
            }
            else if (node is CSharpStatementIRNode)
            {
                context.Errors.OnError(
                    new SourceLocation(documentLocation.AbsoluteIndex, documentLocation.CharacterIndex, documentLocation.Length),
                    LegacyResources.TagHelpers_CodeBlocks_NotSupported_InAttributes,
                    documentLocation.Length);
            }
            else if (node is TemplateIRNode)
            {
                var attributeValueNode = (SetTagHelperPropertyIRNode)node.Parent;
                context.Errors.OnError(
                    new SourceLocation(documentLocation.AbsoluteIndex, documentLocation.CharacterIndex, documentLocation.Length),
                    LegacyResources.FormatTagHelpers_InlineMarkupBlocks_NotSupported_InAttributes(attributeValueNode.Descriptor.TypeName),
                    documentLocation.Length);
            }
        }

        protected class TagHelperContext
        {
            private Dictionary<string, string> _renderedBoundAttributes;
            private HashSet<string> _verifiedPropertyDictionaries;

            public Dictionary<string, string> RenderedBoundAttributes
            {
                get
                {
                    if (_renderedBoundAttributes == null)
                    {
                        _renderedBoundAttributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    return _renderedBoundAttributes;
                }
            }

            public HashSet<string> VerifiedPropertyDictionaries
            {
                get
                {
                    if (_verifiedPropertyDictionaries == null)
                    {
                        _verifiedPropertyDictionaries = new HashSet<string>(StringComparer.Ordinal);
                    }

                    return _verifiedPropertyDictionaries;
                }
            }
        }
    }
}
