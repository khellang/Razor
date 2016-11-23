using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class DirectiveDescriptor
    {
        public DirectiveDescriptorKind Kind { get; set; }
        public string Name { get; set; }
        public IReadOnlyList<DirectiveTokenDescriptor> Tokens { get; set; }
    }

    public class DirectiveTokenDescriptor
    {
        public DirectiveTokenKind Kind { get; set; }
        public string Value { get; set; }
    }

    public enum DirectiveDescriptorKind
    {
        SingleLine = 0,
        RazorBlock = 1,
        CodeBlock = 2
    }

    public enum DirectiveTokenKind
    {
        Type = 0,
        Member = 1,
        String = 2,
        Literal = 3
    }
}