// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.LanguageServer.Adapter;
using Microsoft.CodeAnalysis;
using static Microsoft.AspNetCore.Razor.LanguageServer.ReflectionStrings;

namespace Microsoft.AspNetCore.Razor.LanguageServer
{
    internal static class ProjectLoader
    {
        public static async Task<ProjectServer> LoadAsync(Project project)
        {
            var compilation = await project.GetCompilationAsync();
            var assemblies = GetRazorCustomizationAssemblies(compilation);

            var loadedAssemblies = new List<Assembly>();
            foreach (var assembly in assemblies)
            {
                Assembly loaded;
                if (AssemblyLoader.Default.TryLoadAssembly(project, assembly, out loaded))
                {
                    loadedAssemblies.Add(loaded);
                }
            }

            var adapter = RazorEngineAdapter.Create(loadedAssemblies);

            return new ProjectServer();
        }
    }
}
