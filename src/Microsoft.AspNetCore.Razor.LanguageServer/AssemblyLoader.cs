// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
#if !NET451
using System.Runtime.Loader;
#endif
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Razor.LanguageServer
{
    internal abstract class AssemblyLoader
    {
        public static readonly AssemblyLoader Default = new DefaultAssemblyLoader();

        public abstract bool TryLoadAssembly(Project project, string path, out Assembly assembly);

        private class DefaultAssemblyLoader : AssemblyLoader
        {
            public override bool TryLoadAssembly(Project project, string path, out Assembly assembly)
            {
#if NET451
                try
                {
                    assembly = Assembly.LoadFile(path);
                }
                catch (Exception)
                {
                    assembly = null;
                    return false;
                }

                return true;
#else
                try
                {
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                }
                catch (Exception)
                {
                    assembly = null;
                }

                if (assembly != null)
                {
                    return true;
                }

                // If we fail to load from path, fall back to a name-based load in case it's already
                // in the Load context,

                var assemblyName = AssemblyLoadContext.GetAssemblyName(path);
                if (assemblyName != null)
                {
                    try
                    {
                        assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
                    }
                    catch (Exception)
                    {
                        assembly = null;
                    }
                }

                return assembly != null;
#endif
            }
        }
    }
}
