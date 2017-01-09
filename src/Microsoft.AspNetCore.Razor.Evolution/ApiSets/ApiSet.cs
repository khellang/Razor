// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution.ApiSets
{
    public abstract class ApiSet : IBasicRazorApi
    {
        public static ApiSet Create(IBasicRazorApi basicApi, params IApiExtension[] extensions)
        {
            if (basicApi == null)
            {
                throw new ArgumentNullException(nameof(basicApi));
            }

            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            return new DefaultApiSet(basicApi, extensions);
        }

        public abstract TApiExtension GetExtension<TApiExtension>() where TApiExtension : class, IApiExtension;

        public abstract void WriteChecksum(CSharpWritingContext context, ChecksumIRNode node);
        
        public abstract void WriteClass(CSharpWritingContext context, ClassDeclarationIRNode node);

        public abstract void WriteCSharpAttributeValue(CSharpWritingContext context, CSharpAttributeValueIRNode node);

        public abstract void WriteCSharpExpression(CSharpWritingContext context, CSharpExpressionIRNode node);

        public abstract void WriteCSharpStatement(CSharpWritingContext context, CSharpStatementIRNode node);

        public abstract void WriteDirective(CSharpWritingContext context, DirectiveIRNode node);

        public abstract void WriteDocument(CSharpWritingContext context, DocumentIRNode node);

        public abstract void WriteHtml(CSharpWritingContext context, HtmlContentIRNode node);

        public abstract void WriteHtmlAttribute(CSharpWritingContext context, HtmlAttributeIRNode node);

        public abstract void WriteHtmlAttributeValue(CSharpWritingContext context, HtmlAttributeValueIRNode node);

        public abstract void WriteMethod(CSharpWritingContext context, RazorMethodDeclarationIRNode node);

        public abstract void WriteNamespace(CSharpWritingContext context, NamespaceDeclarationIRNode node);

        public abstract void WriteUsing(CSharpWritingContext context, UsingStatementIRNode node);

        private class DefaultApiSet : ApiSet
        {
            private IBasicRazorApi _basicApi;
            private IApiExtension[] _extensions;

            public DefaultApiSet(IBasicRazorApi basicApi, IApiExtension[] extensions)
            {
                _basicApi = basicApi;

                _extensions = new IApiExtension[extensions.Length + 1];

                _extensions[0] = basicApi;
                for (var i = 0; i < extensions.Length; i++)
                {
                    _extensions[i + 1] = extensions[i];
                }
            }

            public override TApiExtension GetExtension<TApiExtension>()
            {
                for (var i = 0; i < _extensions.Length; i++)
                {
                    var extension = _extensions[i] as TApiExtension;
                    if (extension != null)
                    {
                        return extension;
                    }
                }

                return null;
            }

            public override void WriteChecksum(CSharpWritingContext context, ChecksumIRNode node)
            {
                _basicApi.WriteChecksum(context, node);
            }

            public override void WriteClass(CSharpWritingContext context, ClassDeclarationIRNode node)
            {
                _basicApi.WriteClass(context, node);
            }

            public override void WriteCSharpAttributeValue(CSharpWritingContext context, CSharpAttributeValueIRNode node)
            {
                _basicApi.WriteCSharpAttributeValue(context, node);
            }

            public override void WriteCSharpExpression(CSharpWritingContext context, CSharpExpressionIRNode node)
            {
                _basicApi.WriteCSharpExpression(context, node);
            }

            public override void WriteCSharpStatement(CSharpWritingContext context, CSharpStatementIRNode node)
            {
                _basicApi.WriteCSharpStatement(context, node);
            }

            public override void WriteDirective(CSharpWritingContext context, DirectiveIRNode node)
            {
                _basicApi.WriteDirective(context, node);
            }

            public override void WriteDocument(CSharpWritingContext context, DocumentIRNode node)
            {
                _basicApi.WriteDocument(context, node);
            }

            public override void WriteHtml(CSharpWritingContext context, HtmlContentIRNode node)
            {
                _basicApi.WriteHtml(context, node);
            }

            public override void WriteHtmlAttribute(CSharpWritingContext context, HtmlAttributeIRNode node)
            {
                _basicApi.WriteHtmlAttribute(context, node);
            }

            public override void WriteHtmlAttributeValue(CSharpWritingContext context, HtmlAttributeValueIRNode node)
            {
                _basicApi.WriteHtmlAttributeValue(context, node);
            }

            public override void WriteMethod(CSharpWritingContext context, RazorMethodDeclarationIRNode node)
            {
                _basicApi.WriteMethod(context, node);
            }

            public override void WriteNamespace(CSharpWritingContext context, NamespaceDeclarationIRNode node)
            {
                _basicApi.WriteNamespace(context, node);
            }

            public override void WriteUsing(CSharpWritingContext context, UsingStatementIRNode node)
            {
                _basicApi.WriteUsing(context, node);
            }
        }
    }
}
