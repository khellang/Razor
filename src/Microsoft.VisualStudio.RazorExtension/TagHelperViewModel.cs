using Microsoft.AspNetCore.Razor.Evolution.Legacy;

namespace Microsoft.VisualStudio.RazorExtension
{
    public class TagHelperViewModel : NotifyPropertyChanged
    {
        private readonly TagHelperDescriptor _descriptor;

        internal TagHelperViewModel(TagHelperDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public string AssemblyName => _descriptor.AssemblyName;

        public string TargetElement => _descriptor.TagName;

        public string TypeName => _descriptor.TypeName;
    }
}
