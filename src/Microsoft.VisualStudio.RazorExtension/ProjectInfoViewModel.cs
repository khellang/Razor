using System.Collections.ObjectModel;

namespace Microsoft.VisualStudio.RazorExtension
{
    public class ProjectInfoViewModel : NotifyPropertyChanged
    {
        private ObservableCollection<AssemblyViewModel> _assemblies;
        private ObservableCollection<DirectiveViewModel> _directives;
        private ObservableCollection<DocumentViewModel> _documents;
        private ObservableCollection<TagHelperViewModel> _tagHelpers;

        public ObservableCollection<AssemblyViewModel> Assemblies
        {
            get { return _assemblies; }
            set
            {
                _assemblies = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DirectiveViewModel> Directives
        {
            get { return _directives; }
            set
            {
                _directives = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DocumentViewModel> Documents
        {
            get { return _documents; }
            set
            {
                _documents = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TagHelperViewModel> TagHelpers
        {
            get { return _tagHelpers; }
            set
            {
                _tagHelpers = value;
                OnPropertyChanged();
            }
        }
    }
}
