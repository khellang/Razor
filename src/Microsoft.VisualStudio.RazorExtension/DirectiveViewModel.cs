using System.Text;
using Microsoft.AspNetCore.Razor.Evolution;

namespace Microsoft.VisualStudio.RazorExtension
{
    public class DirectiveViewModel : NotifyPropertyChanged
    {
        private readonly DirectiveDescriptor _directive;

        internal DirectiveViewModel(DirectiveDescriptor directive)
        {
            _directive = directive;

            var builder = new StringBuilder();
            builder.Append("@");
            builder.Append(_directive.Name);

            foreach (var token in _directive.Tokens)
            {
                builder.Append(token.Value);
                builder.Append("(");
                builder.Append(token.Kind.ToString());
                builder.Append(")");
            }

            if (directive.Kind == DirectiveDescriptorKind.CodeBlock || directive.Kind == DirectiveDescriptorKind.RazorBlock)
            {
                builder.Append("{ ... }");
            }

            DisplayText = builder.ToString();
        }

        public string DisplayText { get; }

        public string Name => _directive.Name;
    }
}
