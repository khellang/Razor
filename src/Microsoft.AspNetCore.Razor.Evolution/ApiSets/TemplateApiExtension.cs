// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution.ApiSets
{
    public class TemplateApiExtension : ITemplateApiExtension
    {
        public void WriteTemplate(CSharpWritingContext context, TemplateIRNode node)
        {
            const string ItemParameterName = "item";
            const string TemplateWriterName = "__razor_template_writer";

            context.Writer
                .Write(ItemParameterName).Write(" => ")
                .WriteStartNewObject("Microsoft.AspNetCore.Mvc.Razor.HelperResult" /* ORIGINAL: TemplateTypeName */);

            var textWriter = context.CurrentWriter;
            context.CurrentWriter = TemplateWriterName;

            using (context.Writer.BuildAsyncLambda(endLine: false, parameterNames: TemplateWriterName))
            {
                context.WriteChildren(node);
            }

            context.CurrentWriter = textWriter;

            context.Writer.WriteEndMethodInvocation(endLine: false);
        }
    }
}
