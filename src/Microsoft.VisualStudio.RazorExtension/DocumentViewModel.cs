using Microsoft.CodeAnalysis;

namespace Microsoft.VisualStudio.RazorExtension
{
    public class DocumentViewModel : NotifyPropertyChanged
    {
        public DocumentViewModel(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; }
    }
}