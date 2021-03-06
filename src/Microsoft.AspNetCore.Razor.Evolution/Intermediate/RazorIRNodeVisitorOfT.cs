﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Evolution.Intermediate
{
    public abstract class RazorIRNodeVisitor<TResult>
    {
        public virtual TResult Visit(RazorIRNode node)
        {
            return node.Accept(this);
        }

        public virtual TResult VisitDefault(RazorIRNode node)
        {
            return default(TResult);
        }

        public virtual TResult VisitChecksum(ChecksumIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitToken(RazorIRToken node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitDirectiveToken(DirectiveTokenIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitDirective(DirectiveIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitExtension(ExtensionIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitCSharpStatement(CSharpStatementIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitCSharpExpression(CSharpExpressionIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitHtmlAttributeValue(HtmlAttributeValueIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitCSharpAttributeValue(CSharpAttributeValueIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitHtmlAttribute(HtmlAttributeIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitClass(ClassDeclarationIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitRazorMethodDeclaration(RazorMethodDeclarationIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitDocument(DocumentIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitHtml(HtmlContentIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitNamespace(NamespaceDeclarationIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitUsingStatement(UsingStatementIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitDeclareTagHelperFields(DeclareTagHelperFieldsIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitTagHelper(TagHelperIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitInitializeTagHelperStructure(InitializeTagHelperStructureIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitCreateTagHelper(CreateTagHelperIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitSetTagHelperProperty(SetTagHelperPropertyIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitAddTagHelperHtmlAttribute(AddTagHelperHtmlAttributeIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitExecuteTagHelpers(ExecuteTagHelpersIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitDeclarePreallocatedTagHelperHtmlAttribute(DeclarePreallocatedTagHelperHtmlAttributeIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitAddPreallocatedTagHelperHtmlAttribute(AddPreallocatedTagHelperHtmlAttributeIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitDeclarePreallocatedTagHelperAttribute(DeclarePreallocatedTagHelperAttributeIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitSetPreallocatedTagHelperProperty(SetPreallocatedTagHelperPropertyIRNode node)
        {
            return VisitDefault(node);
        }
    }
}
