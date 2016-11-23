using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Microsoft.CodeAnalysis;
using Razevolution.Tooling;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    [Export(typeof(IRazorEngineTagHelperResolver))]
    internal class DefaultRazorEngineTagHelperResolver : IRazorEngineTagHelperResolver
    {
        public async Task<IEnumerable<TagHelperDescriptor>> GetRazorEngineTagHelpersAsync(Workspace workspace, Project project)
        {
            var compilation = await project.GetCompilationAsync().ConfigureAwait(false);

            var results = new SymbolTableTagHelperDescriptorProvider(compilation).GetTagHelperDescriptors();

            return results;
        }

        public async Task<IEnumerable<TagHelperDescriptor>> GetRazorEngineTagHelpersRemoteAsync(Workspace workspace, Project project)
        {
            var client = await RazorLanguageServiceClientFactory.CreateAsync(workspace, CancellationToken.None);

            using (var session = await client.CreateSessionAsync(project.Solution))
            {
                var results = await session.InvokeAsync<IEnumerable<TagHelperDescriptor>>("GetTagHelpersAsync", new object[] { project.Id.Id, "Foo" }).ConfigureAwait(false);
                return results;
            }
        }
    }
}
