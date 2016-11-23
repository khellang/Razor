using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Microsoft.CodeAnalysis;

namespace Razevolution.Tooling
{
    internal class SymbolTableTagHelperDescriptorProvider
    {
        private static readonly IReadOnlyDictionary<TagHelperDirectiveType, string> _directiveNames = new Dictionary<TagHelperDirectiveType, string>()
        {
            { TagHelperDirectiveType.AddTagHelper, "addTagHelper" },
            { TagHelperDirectiveType.RemoveTagHelper, "removeTagHelper" },
            { TagHelperDirectiveType.TagHelperPrefix, "tagHelperPrefix" },
        };

        private readonly SymbolTableTagHelperDescriptorFactory _descriptorFactory;
        private readonly Dictionary<string, TagHelperDescriptor[]> _cache;
        private readonly INamedTypeSymbol _iTagHelperSymbol;

        public SymbolTableTagHelperDescriptorProvider(Compilation compilation)
        {
            Compilation = compilation;

            _cache = new Dictionary<string, TagHelperDescriptor[]>();
            _descriptorFactory = new SymbolTableTagHelperDescriptorFactory(Compilation, designTime: true);

            _iTagHelperSymbol = Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Razor.TagHelpers.ITagHelper");
        }

        protected Compilation Compilation { get; }

        public IList<TagHelperDescriptor> GetTagHelperDescriptors()
        {
            var results = new List<TagHelperDescriptor>();

            var assemblies = new List<IAssemblySymbol>();
            assemblies.Add(Compilation.Assembly);

            foreach (var reference in Compilation.References)
            {
                var symbol = Compilation.GetAssemblyOrModuleSymbol(reference);
                if (symbol.Kind == SymbolKind.Assembly)
                {
                    assemblies.Add((IAssemblySymbol)symbol);
                }
            }

            foreach (var assembly in assemblies)
            {
                var types = FindTypesImplementing(assembly, _iTagHelperSymbol);

                foreach (var type in types)
                {
                    var descriptors = _descriptorFactory.CreateDescriptors(assembly.Identity.Name, type, new ErrorSink());
                    results.AddRange(descriptors);
                }
            }

            return results;
        }

        private static IAssemblySymbol GetAssembly(Compilation compilation, string assemblyName)
        {
            if (compilation.Assembly.Name == assemblyName)
            {
                return compilation.Assembly;
            }

            foreach (var reference in compilation.References)
            {
                var symbol = compilation.GetAssemblyOrModuleSymbol(reference);
                if (symbol.Name == assemblyName && symbol.Kind == SymbolKind.Assembly)
                {
                    return (IAssemblySymbol)symbol;
                }
            }

            return null;
        }

        private static List<INamedTypeSymbol> FindTypesImplementing(IAssemblySymbol assembly, INamedTypeSymbol type)
        {
            var results = new List<INamedTypeSymbol>();
            FindTypesImplementing(results, assembly.GlobalNamespace, type);
            return results;
        }

        private static void FindTypesImplementing(List<INamedTypeSymbol> results, INamespaceOrTypeSymbol container, INamedTypeSymbol type)
        {
            foreach (var t in container.GetTypeMembers())
            {
                if (t.AllInterfaces.Contains(type))
                {
                    results.Add(t);
                }

                FindTypesImplementing(results, t, type);
            }

            var @namespace = container as INamespaceSymbol;
            if (@namespace != null)
            {
                foreach (var ns in @namespace.GetNamespaceMembers())
                {
                    FindTypesImplementing(results, ns, type);
                }
            }
        }

        private static int GetErrorLength(string directiveText)
        {
            var nonNullLength = directiveText == null ? 1 : directiveText.Length;
            var normalizeEmptyStringLength = Math.Max(nonNullLength, 1);

            return normalizeEmptyStringLength;
        }
    }
}
