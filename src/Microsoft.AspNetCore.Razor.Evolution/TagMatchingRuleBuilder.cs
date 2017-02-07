﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public sealed class TagMatchingRuleBuilder
    {
        private static ICollection<char> InvalidNonWhitespaceTagNameCharacters { get; } = new HashSet<char>(
            new[] { '@', '!', '<', '/', '?', '[', '>', ']', '=', '"', '\'', '*' });

        private string _tagName;
        private string _parentTag;
        private TagStructure _tagStructure;
        private List<RazorDiagnostic> _diagnostics;
        private List<RequiredAttributeDescriptor> _requiredAttributeDescriptors;

        private TagMatchingRuleBuilder()
        {
        }

        public static TagMatchingRuleBuilder Create()
        {
            return new TagMatchingRuleBuilder();
        }

        public TagMatchingRuleBuilder RequireTagName(string tagName)
        {
            _tagName = tagName;

            return this;
        }

        public TagMatchingRuleBuilder RequireParentTag(string parentTag)
        {
            _parentTag = parentTag;

            return this;
        }

        public TagMatchingRuleBuilder RequireTagStructure(TagStructure tagStructure)
        {
            _tagStructure = tagStructure;

            return this;
        }

        public TagMatchingRuleBuilder RequireAttribute(RequiredAttributeDescriptor requiredAttributeDescriptor)
        {
            if (requiredAttributeDescriptor == null)
            {
                throw new ArgumentNullException(nameof(requiredAttributeDescriptor));
            }

            EnsureRequiredAttributeDescriptors();
            _requiredAttributeDescriptors.Add(requiredAttributeDescriptor);

            return this;
        }

        public TagMatchingRuleBuilder RequireAttribute(Action<RequiredAttributeDescriptorBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = RequiredAttributeDescriptorBuilder.Create();

            configure(builder);

            var requiredAttributeDescriptor = builder.Build();

            return RequireAttribute(requiredAttributeDescriptor);
        }

        public TagMatchingRuleBuilder AddDiagnostic(RazorDiagnostic diagnostic)
        {
            EnsureDiagnostics();
            _diagnostics.Add(diagnostic);

            return this;
        }

        public IEnumerable<RazorDiagnostic> Validate()
        {
            if (string.IsNullOrWhiteSpace(_tagName))
            {
                var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                    "TODO: Track IDS",
                    () => "{0} name cannot be null or whitespace.",
                    RazorDiagnosticSeverity.Error);
                var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), _tagName);

                yield return diagnostic;
            }
            else
            {
                foreach (var character in _tagName)
                {
                    if (char.IsWhiteSpace(character) || InvalidNonWhitespaceTagNameCharacters.Contains(character))
                    {
                        var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                            "TODO: Track IDS",
                            () => "Tag helpers cannot target {0} name '{1}' because it contains a '{2}' character.",
                            RazorDiagnosticSeverity.Error);

                        var diagnostic = RazorDiagnostic.Create(
                            diagnosticDescriptor, 
                            new SourceSpan(SourceLocation.Undefined, contentLength: 0), 
                            "Tag".ToLower(), 
                            _tagName, 
                            character);

                        yield return diagnostic;
                    }
                }
            }

            if (_parentTag != null)
            {
                if (string.IsNullOrWhiteSpace(_parentTag))
                {
                    var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                        "TODO: Track IDS",
                        () => "{0} name cannot be null or whitespace.",
                        RazorDiagnosticSeverity.Error);
                    var diagnostic = RazorDiagnostic.Create(diagnosticDescriptor, new SourceSpan(SourceLocation.Undefined, contentLength: 0), "Parent Tag");

                    yield return diagnostic;
                }
                else
                {
                    foreach (var character in _parentTag)
                    {
                        if (char.IsWhiteSpace(character) || InvalidNonWhitespaceTagNameCharacters.Contains(character))
                        {
                            var diagnosticDescriptor = new RazorDiagnosticDescriptor(
                                "TODO: Track IDS",
                                () => "Tag helpers cannot target {0} name '{1}' because it contains a '{2}' character.",
                                RazorDiagnosticSeverity.Error);

                            var diagnostic = RazorDiagnostic.Create(
                                diagnosticDescriptor, 
                                new SourceSpan(SourceLocation.Undefined, contentLength: 0), 
                                "Parent Tag".ToLower(), 
                                _parentTag, 
                                character);

                            yield return diagnostic;
                        }
                    }
                }
            }
        }

        public TagMatchingRule Build()
        {
            var rule = new DefaultTagMatchingRule(
                _tagName,
                _parentTag,
                _tagStructure,
                _requiredAttributeDescriptors ?? Enumerable.Empty<RequiredAttributeDescriptor>(),
                _diagnostics ?? Enumerable.Empty<RazorDiagnostic>());

            return rule;
        }

        private void EnsureRequiredAttributeDescriptors()
        {
            if (_requiredAttributeDescriptors == null)
            {
                _requiredAttributeDescriptors = new List<RequiredAttributeDescriptor>();
            }
        }

        private void EnsureDiagnostics()
        {
            if (_diagnostics == null)
            {
                _diagnostics = new List<RazorDiagnostic>();
            }
        }

        private class DefaultTagMatchingRule : TagMatchingRule
        {
            public DefaultTagMatchingRule(
                string tagName,
                string parentTag,
                TagStructure tagStructure,
                IEnumerable<RequiredAttributeDescriptor> requiredAttributeDescriptors,
                IEnumerable<RazorDiagnostic> diagnostics)
            {
                TagName = tagName;
                ParentTag = parentTag;
                TagStructure = tagStructure;
                Attributes = requiredAttributeDescriptors;
                Diagnostics = diagnostics;
            }
        }
    }
}