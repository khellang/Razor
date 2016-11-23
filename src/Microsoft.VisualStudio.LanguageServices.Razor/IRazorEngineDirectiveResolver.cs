using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.CodeAnalysis;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    internal interface IRazorEngineDirectiveResolver
    {
        Task<IEnumerable<DirectiveDescriptor>> GetRazorEngineDirectivesAsync(Workspace workspace, Project project, CancellationToken cancellationToken = default(CancellationToken));
    }
}
