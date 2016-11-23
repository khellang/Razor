// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.LanguageServer
{
    internal static class ReflectionStrings
    {
        public const string RazorAssemblyName = "Microsoft.AspNetCore.Razor.Evolution";

        public static class RazorEngineCustomizationAttribute
        {
            public const string TypeFullName = RazorAssemblyName + ".RazorEngineCustomizationAttribute";

            public const string FullTypeNameProperty = "TypeFullName";

            public const string MethodNameProperty = "MethodName";
        }

        public static class RazorEngineDependencyAttribute
        {
            public const string TypeFullName = RazorAssemblyName + ".RazorEngineDependencyAttribute";
        }

        public static class IRazorEngineBuilder
        {
            public const string TypeFullName = RazorAssemblyName + ".IRazorEngineBuilder";
        }

        public static class RazorEngine
        {
            public const string TypeFullName = RazorAssemblyName + ".RazorEngine";

            public const string CreateMethodName = "Create";
        }
    }
}
