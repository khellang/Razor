// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using System;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    internal class Tokenizer
    {
        public Tokenizer(ITextDocument source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Source = source;
        }

        public ISymbol CurrentSymbol { get; private set; }

        public SourceLocation CurrentStart { get; private set; }

        public SourceLocation CurrentLocation { get; private set; }

        public ITextDocument Source { get; }
    }
}
