// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Razor.Evolution.Intermediate
{
    public class RazorIRToken : RazorIRNode
    {
        public override IList<RazorIRNode> Children => RazorIRNode.EmptyArray;

        public string Content { get; set; }

        public TokenKind Kind { get; set; } = TokenKind.Unknown;

        public override RazorIRNode Parent { get; set; }

        public override SourceSpan? Source { get; set; }

        public override void Accept(RazorIRNodeVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            visitor.VisitToken(this);
        }

        public override TResult Accept<TResult>(RazorIRNodeVisitor<TResult> visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            return visitor.VisitToken(this);
        }

        [DebuggerDisplay("{DisplayName,nq}")]
        public abstract class TokenKind
        {
            public static readonly TokenKind CSharp = new DefaultTokenKind("CSharp");

            public static readonly TokenKind Html = new DefaultTokenKind("Html");

            public static readonly TokenKind Unknown = new DefaultTokenKind("Unknown");

            public abstract string DisplayName { get; }
        }

        private class DefaultTokenKind : TokenKind
        {
            public DefaultTokenKind(string displayName)
            {
                DisplayName = displayName;
            }

            public override string DisplayName { get; }
        }
    }
}


