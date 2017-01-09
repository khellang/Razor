// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution.ApiSets
{
    public interface IBasicRazorApi : IApiExtension
    {
        void WriteChecksum(CSharpWritingContext context, ChecksumIRNode node);

        void WriteClass(CSharpWritingContext context, ClassDeclarationIRNode node);

        void WriteCSharpAttributeValue(CSharpWritingContext context, CSharpAttributeValueIRNode node);

        void WriteCSharpExpression(CSharpWritingContext context, CSharpExpressionIRNode node);

        void WriteCSharpStatement(CSharpWritingContext context, CSharpStatementIRNode node);

        void WriteDirective(CSharpWritingContext context, DirectiveIRNode node);

        void WriteDocument(CSharpWritingContext context, DocumentIRNode node);

        void WriteHtml(CSharpWritingContext context, HtmlContentIRNode node);

        void WriteHtmlAttribute(CSharpWritingContext context, HtmlAttributeIRNode node);

        void WriteHtmlAttributeValue(CSharpWritingContext context, HtmlAttributeValueIRNode node);

        void WriteMethod(CSharpWritingContext context, RazorMethodDeclarationIRNode node);

        void WriteNamespace(CSharpWritingContext context, NamespaceDeclarationIRNode node);

        void WriteUsing(CSharpWritingContext context, UsingStatementIRNode node);
    }
}
