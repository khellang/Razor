using Microsoft.CodeAnalysis;

namespace Microsoft.VisualStudio.RazorExtension
{
    public class ProjectViewModel : NotifyPropertyChanged
    {
        public ProjectViewModel(Project project)
        {
            Id = project.Id;
            Name = project.Name;
        }

        public string Name { get; }

        public ProjectId Id { get; }
    }
}
