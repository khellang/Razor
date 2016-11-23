using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    [Export(typeof(IRazorEngineDocumentGenerator))]
    internal class DefaultRazorEngineDocumentGenerator : IRazorEngineDocumentGenerator
    {
        public async Task<RazorEngineDocument> GenerateDocumentAsync(Workspace workspace, Project project, string filename, string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = await RazorLanguageServiceClientFactory.CreateAsync(workspace, cancellationToken);

            using (var session = await client.CreateSessionAsync(project.Solution))
            {
                var document = await session.InvokeAsync<RazorEngineDocument>("GenerateDocumentAsync", new object[] { project.Id.Id, "Foo", filename, text }).ConfigureAwait(false);
                return document;
            }
        }
    }
}
