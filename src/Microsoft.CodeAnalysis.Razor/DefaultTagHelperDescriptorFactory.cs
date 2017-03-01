﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Microsoft.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Razor
{
    internal class DefaultTagHelperDescriptorFactory
    {
        private const string DataDashPrefix = "data-";
        private const string TagHelperNameEnding = "TagHelper";
        private const string HtmlCaseRegexReplacement = "-$1$2";
        private const char RequiredAttributeWildcardSuffix = '*';

        // This matches the following AFTER the start of the input string (MATCH).
        // Any letter/number followed by an uppercase letter then lowercase letter: 1(Aa), a(Aa), A(Aa)
        // Any lowercase letter followed by an uppercase letter: a(A)
        // Each match is then prefixed by a "-" via the ToHtmlCase method.
        private static readonly Regex HtmlCaseRegex =
            new Regex(
                "(?<!^)((?<=[a-zA-Z0-9])[A-Z][a-z])|((?<=[a-z])[A-Z])",
                RegexOptions.None,
                TimeSpan.FromMilliseconds(500));

        private readonly INamedTypeSymbol _htmlAttributeNameAttributeSymbol;
        private readonly INamedTypeSymbol _htmlAttributeNotBoundAttributeSymbol;
        private readonly INamedTypeSymbol _htmlTargetElementAttributeSymbol;
        private readonly INamedTypeSymbol _outputElementHintAttributeSymbol;
        private readonly INamedTypeSymbol _iDictionarySymbol;
        private readonly INamedTypeSymbol _restrictChildrenAttributeSymbol;
        private readonly INamedTypeSymbol _editorBrowsableAttributeSymbol;

        public static ICollection<char> InvalidNonWhitespaceNameCharacters { get; } = new HashSet<char>(
            new[] { '@', '!', '<', '/', '?', '[', '>', ']', '=', '"', '\'', '*' });

        private static readonly SymbolDisplayFormat FullNameTypeDisplayFormat =
            SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
                .WithMiscellaneousOptions(SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions & (~SymbolDisplayMiscellaneousOptions.UseSpecialTypes));

        public DefaultTagHelperDescriptorFactory(Compilation compilation, bool designTime)
        {
            DesignTime = designTime;
            _htmlAttributeNameAttributeSymbol = compilation.GetTypeByMetadataName(TagHelperTypes.HtmlAttributeNameAttribute);
            _htmlAttributeNotBoundAttributeSymbol = compilation.GetTypeByMetadataName(TagHelperTypes.HtmlAttributeNotBoundAttribute);
            _htmlTargetElementAttributeSymbol = compilation.GetTypeByMetadataName(TagHelperTypes.HtmlTargetElementAttribute);
            _outputElementHintAttributeSymbol = compilation.GetTypeByMetadataName(TagHelperTypes.OutputElementHintAttribute);
            _restrictChildrenAttributeSymbol = compilation.GetTypeByMetadataName(TagHelperTypes.RestrictChildrenAttribute);
            _editorBrowsableAttributeSymbol = compilation.GetTypeByMetadataName(typeof(EditorBrowsableAttribute).FullName);
            _iDictionarySymbol = compilation.GetTypeByMetadataName(TagHelperTypes.IDictionary);
        }

        protected bool DesignTime { get; }

        /// <inheritdoc />
        public virtual TagHelperDescriptor CreateDescriptor(INamedTypeSymbol type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (ShouldSkipDescriptorCreation(type))
            {
                return null;
            }

            var typeName = GetFullName(type);
            var assemblyName = type.ContainingAssembly.Identity.Name;
            var descriptorBuilder = ITagHelperDescriptorBuilder.Create(typeName, assemblyName);

            AddBoundAttributes(type, descriptorBuilder);
            AddTagMatchingRules(type, descriptorBuilder);
            AddAllowedChildren(type, descriptorBuilder);
            AddDocumentation(type, descriptorBuilder);
            AddTagOutputHint(type, descriptorBuilder);

            var diagnostics = descriptorBuilder.Validate();
            foreach (var diagnostic in diagnostics)
            {
                descriptorBuilder.AddDiagnostic(diagnostic);
            }

            var descriptor = descriptorBuilder.Build();

            return descriptor;
        }

        private void AddTagMatchingRules(INamedTypeSymbol type, ITagHelperDescriptorBuilder descriptorBuilder)
        {
            var targetElementAttributes = type
                .GetAttributes()
                .Where(attribute => attribute.AttributeClass == _htmlTargetElementAttributeSymbol);

            // If there isn't an attribute specifying the tag name derive it from the name
            if (!targetElementAttributes.Any())
            {
                var name = type.Name;

                if (name.EndsWith(TagHelperNameEnding, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - TagHelperNameEnding.Length);
                }

                descriptorBuilder.TagMatchingRule(ruleBuilder =>
                {
                    var htmlCasedName = ToHtmlCase(name);
                    ruleBuilder.RequireTagName(htmlCasedName);

                    var diagnostics = ruleBuilder.Validate();
                    foreach (var diagnostic in diagnostics)
                    {
                        ruleBuilder.AddDiagnostic(diagnostic);
                    }
                });

                return;
            }

            foreach (var targetElementAttribute in targetElementAttributes)
            {
                descriptorBuilder.TagMatchingRule(ruleBuilder =>
                {
                    var tagName = HtmlTargetElementAttribute_Tag(targetElementAttribute);
                    ruleBuilder.RequireTagName(tagName);

                    var parentTag = HtmlTargetElementAttribute_ParentTag(targetElementAttribute);
                    ruleBuilder.RequireParentTag(parentTag);

                    var tagStructure = HtmlTargetElementAttribute_TagStructure(targetElementAttribute);
                    ruleBuilder.RequireTagStructure(tagStructure);

                    var requiredAttributeString = HtmlTargetElementAttribute_Attributes(targetElementAttribute);
                    RequiredAttributeParser.AddRequiredAttributes(requiredAttributeString, ruleBuilder);

                    var diagnostics = ruleBuilder.Validate();
                    foreach (var diagnostic in diagnostics)
                    {
                        ruleBuilder.AddDiagnostic(diagnostic);
                    }
                });
            }
        }

        private void AddBoundAttributes(INamedTypeSymbol type, ITagHelperDescriptorBuilder builder)
        {
            var accessibleProperties = GetAccessibleProperties(type);
            foreach (var property in accessibleProperties)
            {
                if (ShouldSkipDescriptorCreation(property))
                {
                    continue;
                }

                builder.BindAttribute(attributeBuilder =>
                {
                    ConfigureBoundAttribute(attributeBuilder, property, type);

                    var diagnostics = attributeBuilder.Validate();
                    foreach (var diagnostic in diagnostics)
                    {
                        attributeBuilder.AddDiagnostic(diagnostic);
                    }
                });
            }
        }

        private void AddAllowedChildren(INamedTypeSymbol type, ITagHelperDescriptorBuilder builder)
        {
            var restrictChildrenAttribute = type.GetAttributes().Where(a => a.AttributeClass == _restrictChildrenAttributeSymbol).FirstOrDefault();
            if (restrictChildrenAttribute == null)
            {
                return;
            }

            builder.AllowChildTag((string)restrictChildrenAttribute.ConstructorArguments[0].Value);

            if (restrictChildrenAttribute.ConstructorArguments.Length == 2)
            {
                foreach (var value in restrictChildrenAttribute.ConstructorArguments[1].Values)
                {
                    builder.AllowChildTag((string)value.Value);
                }
            }
        }

        private void AddDocumentation(INamedTypeSymbol type, ITagHelperDescriptorBuilder builder)
        {
            if (!DesignTime)
            {
                return;
            }

            var xml = type.GetDocumentationCommentXml();

            builder.Documentation(xml);
        }

        private void AddTagOutputHint(INamedTypeSymbol type, ITagHelperDescriptorBuilder builder)
        {
            if (!DesignTime)
            {
                return;
            }
            string outputElementHint = null;
            var outputElementHintAttribute = type.GetAttributes().Where(a => a.AttributeClass == _outputElementHintAttributeSymbol).FirstOrDefault();
            if (outputElementHintAttribute != null)
            {
                outputElementHint = (string)(outputElementHintAttribute.ConstructorArguments[0]).Value;
                builder.TagOutputHint(outputElementHint);
            }
        }

        public void ConfigureBoundAttribute(
            ITagHelperBoundAttributeDescriptorBuilder builder,
            IPropertySymbol property,
            INamedTypeSymbol containingType)
        {
            var attributeNameAttribute = property
                    .GetAttributes()
                    .Where(a => a.AttributeClass == _htmlAttributeNameAttributeSymbol)
                    .FirstOrDefault();

            bool hasExplicitName;
            string attributeName;
            if (attributeNameAttribute == null ||
                attributeNameAttribute.ConstructorArguments.Length == 0 ||
                string.IsNullOrEmpty((string)attributeNameAttribute.ConstructorArguments[0].Value))
            {
                hasExplicitName = false;
                attributeName = ToHtmlCase(property.Name);
            }
            else
            {
                hasExplicitName = true;
                attributeName = (string)attributeNameAttribute.ConstructorArguments[0].Value;
            }

            if (property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public)
            {
                var typeName = GetFullName(property.Type);
                builder
                    .Name(attributeName)
                    .PropertyName(property.Name)
                    .TypeName(typeName);

                if (property.Type.TypeKind == TypeKind.Enum)
                {
                    builder.AsEnum();
                }

                if (DesignTime)
                {
                    var xml = property.GetDocumentationCommentXml();

                    builder.Documentation(xml);
                }
            }
            else if (hasExplicitName)
            {
                // Specified HtmlAttributeNameAttribute.Name though property has no public setter.
                var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                    "TODO: Track IDS",
                    () => Resources.TagHelperDescriptorFactory_InvalidAttributeNameNotNullOrEmpty,
                    RazorDiagnosticSeverity.Error);
                var diagnostic = RazorDiagnostic.Create(
                    diagnosticDescriptor,
                    new SourceSpan(SourceLocation.Undefined, contentLength: 0),
                    GetFullName(containingType),
                    property.Name,
                    TagHelperTypes.HtmlAttributeNameAttribute,
                    TagHelperTypes.HtmlAttributeName.Name);
                builder.AddDiagnostic(diagnostic);
            }

            var hasPublicSetter = property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public;

            string dictionaryAttributePrefix = null;
            var dictionaryAttributePrefixSet = false;

            if (attributeNameAttribute != null)
            {
                foreach (var argument in attributeNameAttribute.NamedArguments)
                {
                    if (argument.Key == TagHelperTypes.HtmlAttributeName.DictionaryAttributePrefix)
                    {
                        dictionaryAttributePrefix = (string)argument.Value.Value;
                        dictionaryAttributePrefixSet = true;
                        break;
                    }
                }
            }

            INamedTypeSymbol dictionaryType;
            if ((property.Type as INamedTypeSymbol)?.ConstructedFrom == _iDictionarySymbol)
            {
                dictionaryType = (INamedTypeSymbol)property.Type;
            }
            else if (property.Type.AllInterfaces.Any(s => s.ConstructedFrom == _iDictionarySymbol))
            {
                dictionaryType = property.Type.AllInterfaces.First(s => s.ConstructedFrom == _iDictionarySymbol);
            }
            else
            {
                dictionaryType = null;
            }

            var dictionaryValueType = dictionaryType?.TypeArguments[0];

            if (dictionaryValueType?.SpecialType != SpecialType.System_String)
            {
                if (dictionaryAttributePrefix != null)
                {
                    // DictionaryAttributePrefix is not supported unless associated with an
                    // IDictionary<string, TValue> property.
                    var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                        "TODO: Track IDS",
                        () => Resources.TagHelperDescriptorFactory_InvalidAttributePrefixNotNull,
                        RazorDiagnosticSeverity.Error);
                    var diagnostic = RazorDiagnostic.Create(
                        diagnosticDescriptor,
                        new SourceSpan(SourceLocation.Undefined, contentLength: 0),
                        GetFullName(containingType),
                        property.Name,
                        TagHelperTypes.HtmlAttributeNameAttribute,
                        TagHelperTypes.HtmlAttributeName.DictionaryAttributePrefix,
                        "IDictionary<string, TValue>");
                    builder.AddDiagnostic(diagnostic);
                }

                return;
            }
            else if (!hasPublicSetter && attributeNameAttribute != null && !dictionaryAttributePrefixSet)
            {
                // Must set DictionaryAttributePrefix when using HtmlAttributeNameAttribute with a dictionary property
                // that lacks a public setter.
                var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                    "TODO: Track IDS",
                    () => Resources.TagHelperDescriptorFactory_InvalidAttributePrefixNull,
                    RazorDiagnosticSeverity.Error);
                var diagnostic = RazorDiagnostic.Create(
                    diagnosticDescriptor,
                    new SourceSpan(SourceLocation.Undefined, contentLength: 0),
                    GetFullName(containingType),
                    property.Name,
                    TagHelperTypes.HtmlAttributeNameAttribute,
                    TagHelperTypes.HtmlAttributeName.DictionaryAttributePrefix,
                    "IDictionary<string, TValue>");
                builder.AddDiagnostic(diagnostic);

                return;
            }

            // Potential prefix case. Use default prefix (based on name)?
            var useDefault = attributeNameAttribute == null || !dictionaryAttributePrefixSet;

            var prefix = useDefault ? attributeName + "-" : dictionaryAttributePrefix;
            if (prefix == null)
            {
                // DictionaryAttributePrefix explicitly set to null. No need to mutate the descriptor to represent indexers.
                return;
            }

            var dictionaryValueTypeName = GetFullName(dictionaryValueType);
            builder.AsDictionary(prefix, dictionaryValueTypeName);
        }

        private static string HtmlTargetElementAttribute_Attributes(AttributeData attibute)
        {
            foreach (var kvp in attibute.NamedArguments)
            {
                if (kvp.Key == TagHelperTypes.HtmlTargetElement.Attributes)
                {
                    return (string)kvp.Value.Value;
                }
            }

            return null;
        }

        private static string HtmlTargetElementAttribute_ParentTag(AttributeData attibute)
        {
            foreach (var kvp in attibute.NamedArguments)
            {
                if (kvp.Key == TagHelperTypes.HtmlTargetElement.ParentTag)
                {
                    return (string)kvp.Value.Value;
                }
            }

            return null;
        }

        private static string HtmlTargetElementAttribute_Tag(AttributeData attibute)
        {
            if (attibute.ConstructorArguments.Length == 0)
            {
                return TagHelperDescriptorProvider.ElementCatchAllTarget;
            }
            else
            {
                return (string)attibute.ConstructorArguments[0].Value;
            }
        }

        private static TagStructure HtmlTargetElementAttribute_TagStructure(AttributeData attibute)
        {
            foreach (var kvp in attibute.NamedArguments)
            {
                if (kvp.Key == TagHelperTypes.HtmlTargetElement.TagStructure)
                {
                    return (TagStructure)kvp.Value.Value;
                }
            }

            return TagStructure.Unspecified;
        }

        private IEnumerable<IPropertySymbol> GetAccessibleProperties(INamedTypeSymbol typeSymbol)
        {
            var accessibleProperties = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);
            do
            {
                var members = typeSymbol.GetMembers();
                for (var i = 0; i < members.Length; i++)
                {
                    var property = members[i] as IPropertySymbol;
                    if (property != null &&
                        property.Parameters.Length == 0 &&
                        property.GetMethod != null &&
                        property.GetMethod.DeclaredAccessibility == Accessibility.Public &&
                        property.GetAttributes().Where(a => a.AttributeClass == _htmlAttributeNotBoundAttributeSymbol).FirstOrDefault() == null &&
                        !accessibleProperties.ContainsKey(property.Name))
                    {
                        accessibleProperties.Add(property.Name, property);
                    }
                }

                typeSymbol = typeSymbol.BaseType;
            }
            while (typeSymbol != null);

            return accessibleProperties.Values;
        }

        private bool ShouldSkipDescriptorCreation(ISymbol symbol)
        {
            if (DesignTime)
            {
                var editorBrowsableAttribute = symbol.GetAttributes().Where(a => a.AttributeClass == _editorBrowsableAttributeSymbol).FirstOrDefault();

                if (editorBrowsableAttribute == null)
                {
                    return false;
                }

                if (editorBrowsableAttribute.ConstructorArguments.Length > 0)
                {
                    return (EditorBrowsableState)editorBrowsableAttribute.ConstructorArguments[0].Value == EditorBrowsableState.Never;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts from pascal/camel case to lower kebab-case.
        /// </summary>
        /// <example>
        /// SomeThing => some-thing
        /// capsONInside => caps-on-inside
        /// CAPSOnOUTSIDE => caps-on-outside
        /// ALLCAPS => allcaps
        /// One1Two2Three3 => one1-two2-three3
        /// ONE1TWO2THREE3 => one1two2three3
        /// First_Second_ThirdHi => first_second_third-hi
        /// </example>
        internal static string ToHtmlCase(string name)
        {
            return HtmlCaseRegex.Replace(name, HtmlCaseRegexReplacement).ToLowerInvariant();
        }

        private static string GetFullName(ITypeSymbol type) => type.ToDisplayString(FullNameTypeDisplayFormat);

        // Internal for testing
        internal static class RequiredAttributeParser
        {
            public static void AddRequiredAttributes(string requiredAttributes, TagMatchingRuleBuilder ruleBuilder)
            {
                var requiredAttributeParser = new DefaultRequiredAttributeParser(requiredAttributes);
                requiredAttributeParser.AddRequiredAttributes(ruleBuilder);
            }

            private class DefaultRequiredAttributeParser
            {
                private static readonly IReadOnlyDictionary<char, RequiredAttributeDescriptor.ValueComparisonMode> CssValueComparisons =
                    new Dictionary<char, RequiredAttributeDescriptor.ValueComparisonMode>
                    {
                        { '=', RequiredAttributeDescriptor.ValueComparisonMode.FullMatch },
                        { '^', RequiredAttributeDescriptor.ValueComparisonMode.PrefixMatch },
                        { '$', RequiredAttributeDescriptor.ValueComparisonMode.SuffixMatch }
                    };
                private static readonly char[] InvalidPlainAttributeNameCharacters = { ' ', '\t', ',', RequiredAttributeWildcardSuffix };
                private static readonly char[] InvalidCssAttributeNameCharacters = (new[] { ' ', '\t', ',', ']' })
                    .Concat(CssValueComparisons.Keys)
                    .ToArray();
                private static readonly char[] InvalidCssQuotelessValueCharacters = { ' ', '\t', ']' };

                private int _index;
                private string _requiredAttributes;

                public DefaultRequiredAttributeParser(string requiredAttributes)
                {
                    _requiredAttributes = requiredAttributes;
                }

                private char Current => _requiredAttributes[_index];

                private bool AtEnd => _index >= _requiredAttributes.Length;

                public void AddRequiredAttributes(TagMatchingRuleBuilder ruleBuilder)
                {
                    if (string.IsNullOrEmpty(_requiredAttributes))
                    {
                        return;
                    }
                    var descriptors = new List<RequiredAttributeDescriptor>();

                    PassOptionalWhitespace();

                    do
                    {
                        ruleBuilder.RequireAttribute(attributeBuilder =>
                        {
                            if (At('['))
                            {
                                ParseCssSelector(attributeBuilder);
                            }
                            else
                            {
                                ParsePlainSelector(attributeBuilder);
                            }

                            PassOptionalWhitespace();

                            if (At(','))
                            {
                                _index++;

                                if (!EnsureNotAtEnd(attributeBuilder))
                                {
                                    return;
                                }
                            }
                            else if (!AtEnd)
                            {
                                var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                                    "TODO: Track IDS",
                                    () => Resources.TagHelperDescriptorFactory_InvalidRequiredAttributeCharacter,
                                    RazorDiagnosticSeverity.Error);
                                var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), Current, _requiredAttributes);
                                ruleBuilder.AddDiagnostic(diagnostic);
                                return;
                            }

                            PassOptionalWhitespace();

                            var diagnostics = attributeBuilder.Validate();
                            foreach (var diagnostic in diagnostics)
                            {
                                attributeBuilder.AddDiagnostic(diagnostic);
                            }
                        });
                    }
                    while (!AtEnd);
                }

                private void ParsePlainSelector(RequiredAttributeDescriptorBuilder attributeBuilder)
                {
                    var nameEndIndex = _requiredAttributes.IndexOfAny(InvalidPlainAttributeNameCharacters, _index);
                    string attributeName;

                    var nameComparison = RequiredAttributeDescriptor.NameComparisonMode.FullMatch;
                    if (nameEndIndex == -1)
                    {
                        attributeName = _requiredAttributes.Substring(_index);
                        _index = _requiredAttributes.Length;
                    }
                    else
                    {
                        attributeName = _requiredAttributes.Substring(_index, nameEndIndex - _index);
                        _index = nameEndIndex;

                        if (_requiredAttributes[nameEndIndex] == RequiredAttributeWildcardSuffix)
                        {
                            nameComparison = RequiredAttributeDescriptor.NameComparisonMode.PrefixMatch;

                            // Move past wild card
                            _index++;
                        }
                    }

                    attributeBuilder.Name(attributeName);
                    attributeBuilder.NameComparisonMode(nameComparison);
                }

                private void ParseCssAttributeName(RequiredAttributeDescriptorBuilder builder)
                {
                    var nameStartIndex = _index;
                    var nameEndIndex = _requiredAttributes.IndexOfAny(InvalidCssAttributeNameCharacters, _index);
                    nameEndIndex = nameEndIndex == -1 ? _requiredAttributes.Length : nameEndIndex;
                    _index = nameEndIndex;

                    var attributeName = _requiredAttributes.Substring(nameStartIndex, nameEndIndex - nameStartIndex);

                    builder.Name(attributeName);
                }

                private void ParseCssValueComparison(RequiredAttributeDescriptorBuilder builder)
                {
                    Debug.Assert(!AtEnd);
                    RequiredAttributeDescriptor.ValueComparisonMode valueComparison;

                    if (CssValueComparisons.TryGetValue(Current, out valueComparison))
                    {
                        var op = Current;
                        _index++;

                        if (op != '=' && At('='))
                        {
                            // Two length operator (ex: ^=). Move past the second piece
                            _index++;
                        }
                        else if (op != '=') // We're at an incomplete operator (ex: [foo^]
                        {
                            var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                                "TODO: Track IDS",
                                () => Resources.TagHelperDescriptorFactory_PartialRequiredAttributeOperator,
                                RazorDiagnosticSeverity.Error);
                            var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), _requiredAttributes, op);
                            builder.AddDiagnostic(diagnostic);

                            return;
                        }
                    }
                    else if (!At(']'))
                    {
                        var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                            "TODO: Track IDS",
                            () => Resources.TagHelperDescriptorFactory_InvalidRequiredAttributeOperator,
                            RazorDiagnosticSeverity.Error);
                        var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), Current, _requiredAttributes);
                        builder.AddDiagnostic(diagnostic);

                        return;
                    }

                    builder.ValueComparison(valueComparison);
                }

                private void ParseCssValue(RequiredAttributeDescriptorBuilder builder)
                {
                    int valueStart;
                    int valueEnd;
                    if (At('\'') || At('"'))
                    {
                        var quote = Current;

                        // Move past the quote
                        _index++;

                        valueStart = _index;
                        valueEnd = _requiredAttributes.IndexOf(quote, _index);
                        if (valueEnd == -1)
                        {
                            var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                                "TODO: Track IDS",
                                () => Resources.TagHelperDescriptorFactory_InvalidRequiredAttributeMismatchedQuotes,
                            RazorDiagnosticSeverity.Error);
                            var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), _requiredAttributes, quote);
                            builder.AddDiagnostic(diagnostic);

                            return;
                        }
                        _index = valueEnd + 1;
                    }
                    else
                    {
                        valueStart = _index;
                        var valueEndIndex = _requiredAttributes.IndexOfAny(InvalidCssQuotelessValueCharacters, _index);
                        valueEnd = valueEndIndex == -1 ? _requiredAttributes.Length : valueEndIndex;
                        _index = valueEnd;
                    }

                    var value = _requiredAttributes.Substring(valueStart, valueEnd - valueStart);

                    builder.Value(value);
                }

                private void ParseCssSelector(RequiredAttributeDescriptorBuilder attributeBuilder)
                {
                    Debug.Assert(At('['));

                    // Move past '['.
                    _index++;
                    PassOptionalWhitespace();

                    ParseCssAttributeName(attributeBuilder);

                    PassOptionalWhitespace();

                    if (!EnsureNotAtEnd(attributeBuilder))
                    {
                        return;
                    }

                    ParseCssValueComparison(attributeBuilder);

                    PassOptionalWhitespace();

                    if (!EnsureNotAtEnd(attributeBuilder))
                    {
                        return;
                    }

                    ParseCssValue(attributeBuilder);

                    PassOptionalWhitespace();

                    if (At(']'))
                    {
                        // Move past the ending bracket.
                        _index++;
                    }
                    else if (AtEnd)
                    {
                        var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                            "TODO: Track IDS",
                            () => Resources.TagHelperDescriptorFactory_CouldNotFindMatchingEndBrace,
                            RazorDiagnosticSeverity.Error);
                        var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), _requiredAttributes);
                        attributeBuilder.AddDiagnostic(diagnostic);

                        return;
                    }
                    else
                    {
                        var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                            "TODO: Track IDS",
                            () => Resources.TagHelperDescriptorFactory_InvalidRequiredAttributeCharacter,
                            RazorDiagnosticSeverity.Error);
                        var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), Current, _requiredAttributes);
                        attributeBuilder.AddDiagnostic(diagnostic);

                        return;
                    }
                }

                private bool EnsureNotAtEnd(RequiredAttributeDescriptorBuilder builder)
                {
                    if (AtEnd)
                    {
                        var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                            "TODO: Track IDS",
                            () => Resources.TagHelperDescriptorFactory_CouldNotFindMatchingEndBrace,
                            RazorDiagnosticSeverity.Error);
                        var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), _requiredAttributes);
                        builder.AddDiagnostic(diagnostic);

                        return false;
                    }

                    return true;
                }

                private bool At(char c)
                {
                    return !AtEnd && Current == c;
                }

                private void PassOptionalWhitespace()
                {
                    while (!AtEnd && (Current == ' ' || Current == '\t'))
                    {
                        _index++;
                    }
                }
            }
        }
    }
}