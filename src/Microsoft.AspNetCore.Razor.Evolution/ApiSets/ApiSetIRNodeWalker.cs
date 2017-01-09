// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution.ApiSets
{
    public sealed class ApiSetIRNodeWalker : RazorIRNodeWalker
    {
        private readonly ApiSet _apiSet;
        private readonly CSharpWritingContext _context;

        public ApiSetIRNodeWalker(ApiSet apiSet, CSharpWritingContext context)
        {
            _apiSet = apiSet;
            _context = context;
        }

        public override void VisitChecksum(ChecksumIRNode node)
        {
            _apiSet.WriteChecksum(_context, node);
        }

        public override void VisitClass(ClassDeclarationIRNode node)
        {
            _apiSet.WriteClass(_context, node);
        }

        public override void VisitCSharpAttributeValue(CSharpAttributeValueIRNode node)
        {
            _apiSet.WriteCSharpAttributeValue(_context, node);
        }

        public override void VisitCSharpExpression(CSharpExpressionIRNode node)
        {
            _apiSet.WriteCSharpExpression(_context, node);
        }

        public override void VisitCSharpStatement(CSharpStatementIRNode node)
        {
            _apiSet.WriteCSharpStatement(_context, node);
        }

        public override void VisitDirective(DirectiveIRNode node)
        {
            _apiSet.WriteDirective(_context, node);
        }

        public override void VisitDocument(DocumentIRNode node)
        {
            _apiSet.WriteDocument(_context, node);
        }

        public override void VisitExtension(ExtensionIRNode node)
        {
            node.Accept(_apiSet);
        }

        public override void VisitHtml(HtmlContentIRNode node)
        {
            _apiSet.WriteHtml(_context, node);
        }

        public override void VisitHtmlAttribute(HtmlAttributeIRNode node)
        {
            _apiSet.WriteHtmlAttribute(_context, node);
        }

        public override void VisitHtmlAttributeValue(HtmlAttributeValueIRNode node)
        {
            _apiSet.WriteHtmlAttributeValue(_context, node);
        }

        public override void VisitNamespace(NamespaceDeclarationIRNode node)
        {
            _apiSet.WriteNamespace(_context, node);
        }

        public override void VisitRazorMethodDeclaration(RazorMethodDeclarationIRNode node)
        {
            _apiSet.WriteMethod(_context, node);
        }

        public override void VisitUsingStatement(UsingStatementIRNode node)
        {
            _apiSet.WriteUsing(_context, node);
        }




        public override void VisitAddPreallocatedTagHelperHtmlAttribute(AddPreallocatedTagHelperHtmlAttributeIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteAddPreallocatedTagHelperHtmlAttribute(_context, node);
        }

        public override void VisitAddTagHelperHtmlAttribute(AddTagHelperHtmlAttributeIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteAddTagHelperHtmlAttribute(_context, node);
        }

        public override void VisitCreateTagHelper(CreateTagHelperIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteCreateTagHelper(_context, node);
        }

        public override void VisitDeclarePreallocatedTagHelperAttribute(DeclarePreallocatedTagHelperAttributeIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteDeclarePreallocatedTagHelperAttribute(_context, node);
        }

        public override void VisitDeclarePreallocatedTagHelperHtmlAttribute(DeclarePreallocatedTagHelperHtmlAttributeIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteDeclarePreallocatedTagHelperHtmlAttribute(_context, node);
        }

        public override void VisitDeclareTagHelperFields(DeclareTagHelperFieldsIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteDeclareTagHelperFields(_context, node);
        }

        public override void VisitExecuteTagHelpers(ExecuteTagHelpersIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteExecuteTagHelpers(_context, node);
        }

        public override void VisitInitializeTagHelperStructure(InitializeTagHelperStructureIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteInitializeTagHelperStructure(_context, node);
        }

        public override void VisitSetPreallocatedTagHelperProperty(SetPreallocatedTagHelperPropertyIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteSetPreallocatedTagHelperProperty(_context, node);
        }

        public override void VisitSetTagHelperProperty(SetTagHelperPropertyIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteSetTagHelperProperty(_context, node);
        }

        public override void VisitTagHelper(TagHelperIRNode node)
        {
            _apiSet.GetExtension<ITagHelperApiExtension>().WriteTagHelper(_context, node);
        }

        public override void VisitTemplate(TemplateIRNode node)
        {
            _apiSet.GetExtension<ITemplateApiExtension>().WriteTemplate(_context, node);
        }
    }
}
