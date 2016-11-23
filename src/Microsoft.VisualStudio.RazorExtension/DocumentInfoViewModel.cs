using Microsoft.VisualStudio.LanguageServices.Razor;

namespace Microsoft.VisualStudio.RazorExtension
{
    public class DocumentInfoViewModel : NotifyPropertyChanged
    {
        private RazorEngineDocument _document;

        internal DocumentInfoViewModel(RazorEngineDocument document)
        {
            _document = document;
        }

        public string Text => _document.Text;
    }
}