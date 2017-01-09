// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Evolution.ApiSets;

namespace Microsoft.AspNetCore.Razor.Evolution.Intermediate
{
    public abstract class ExtensionIRNode : RazorIRNode
    {
        public abstract void Accept(ApiSet apiSet);

        public sealed override void Accept(RazorIRNodeVisitor visitor)
        {
            visitor.VisitExtension(this);
        }

        public sealed override TResult Accept<TResult>(RazorIRNodeVisitor<TResult> visitor)
        {
            return visitor.VisitExtension(this);
        }
    }
}
