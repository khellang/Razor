// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Razor
{
    public abstract class TagHelper
    {
        public abstract IReadOnlyList<TagHelperAttribute> Attributes { get; }

        public abstract string DisplayName { get; }

        public abstract IReadOnlyDictionary<string, string> Properties { get; }

        public abstract IReadOnlyList<TagHelperRule> Rules { get; }

        public abstract TagStructure TagStructure { get; }
    }
}
