using Microsoft.VisualStudio.LanguageServices.Razor;

namespace Microsoft.VisualStudio.RazorExtension
{
    public class AssemblyViewModel : NotifyPropertyChanged
    {
        private readonly RazorEngineAssembly _assembly;

        internal AssemblyViewModel(RazorEngineAssembly assembly)
        {
            _assembly = assembly;

            Name = _assembly.Identity.GetDisplayName();
            FilePath = assembly.FilePath;
        }

        public string Name { get; }

        public string FilePath { get; }
    }
}
