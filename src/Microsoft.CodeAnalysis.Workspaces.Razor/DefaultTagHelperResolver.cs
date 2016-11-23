// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Razor
{
    internal class DefaultTagHelperResolver : TagHelperResolver
    {
        public override async Task<IReadOnlyList<TagHelper>> GetTagHelpersAsync(Project project, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            var results = new List<TagHelper>();

            var @interface = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper");
            if (@interface == null)
            {
                return results;
            }

            var types = new List<INamedTypeSymbol>();
            var visitor = new TagHelperTypeVisitor(@interface, types);

            visitor.Visit(compilation.Assembly.GlobalNamespace);

            foreach (var reference in compilation.References)
            {
                var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                if (assembly != null)
                {
                    visitor.Visit(compilation.Assembly.GlobalNamespace);
                }
            }



            return results;
        }

        // Visitor for top-level types.
        private class TagHelperTypeVisitor : SymbolVisitor
        {
            private INamedTypeSymbol _interface;
            private List<INamedTypeSymbol> _results;

            public TagHelperTypeVisitor(INamedTypeSymbol @interface, List<INamedTypeSymbol> results)
            {
                _interface = @interface;
                _results = results;
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (symbol.AllInterfaces.Contains(_interface))
                {
                    _results.Add(symbol);
                }
            }

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var member in symbol.GetMembers())
                {
                    Visit(member);
                }
            }
        }
    }
}
