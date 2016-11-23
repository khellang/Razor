// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis.Razor
{
    public abstract class TagHelperResolver : ILanguageService
    {
        public abstract Task<IReadOnlyList<TagHelper>> GetTagHelpersAsync(Project project, CancellationToken cancellationToken = default(CancellationToken));
    }
}
