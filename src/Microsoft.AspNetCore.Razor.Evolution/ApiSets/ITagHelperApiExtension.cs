// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution.ApiSets
{
    public interface ITagHelperApiExtension : IApiExtension
    {
        void WriteDeclareTagHelperFields(CSharpWritingContext context, DeclareTagHelperFieldsIRNode node);

        void WriteTagHelper(CSharpWritingContext context, TagHelperIRNode node);

        void WriteInitializeTagHelperStructure(CSharpWritingContext context, InitializeTagHelperStructureIRNode node);

        void WriteCreateTagHelper(CSharpWritingContext context, CreateTagHelperIRNode node);

        void WriteSetTagHelperProperty(CSharpWritingContext context, SetTagHelperPropertyIRNode node);

        void WriteAddTagHelperHtmlAttribute(CSharpWritingContext context, AddTagHelperHtmlAttributeIRNode node);

        void WriteExecuteTagHelpers(CSharpWritingContext context, ExecuteTagHelpersIRNode node);

        void WriteDeclarePreallocatedTagHelperHtmlAttribute(CSharpWritingContext context, DeclarePreallocatedTagHelperHtmlAttributeIRNode node);

        void WriteAddPreallocatedTagHelperHtmlAttribute(CSharpWritingContext context, AddPreallocatedTagHelperHtmlAttributeIRNode node);

        void WriteDeclarePreallocatedTagHelperAttribute(CSharpWritingContext context, DeclarePreallocatedTagHelperAttributeIRNode node);

        void WriteSetPreallocatedTagHelperProperty(CSharpWritingContext context, SetPreallocatedTagHelperPropertyIRNode node);
    }
}
