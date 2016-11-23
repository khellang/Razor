// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.AspNetCore.Razor.LanguageServer.ReflectionStrings;

namespace Microsoft.AspNetCore.Razor.LanguageServer.Adapter
{
    // Acts as an adapter to the 'real' RazorEngine. This way the language server doesn't reference
    // or load any specific version of Razor.
    internal abstract class RazorEngineAdapter
    {
        public static RazorEngineAdapter Create(IEnumerable<Assembly> assemblies)
        {
            var razorAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == RazorAssemblyName);
            if (razorAssembly == null)
            {
                throw new InvalidOperationException("This project doesn't reference Razor.");
            }

            var razorEngineBuilderType = razorAssembly.GetType(IRazorEngineBuilder.TypeFullName, throwOnError: true);
            var configureMethodParameters = new Type[] { typeof(Action<>).MakeGenericType(razorEngineBuilderType) };

            var customizationAttribute = razorAssembly.GetType(RazorEngineCustomizationAttribute.TypeFullName, throwOnError: true);

            var configureMethods = new List<MethodInfo>();
            foreach (var assembly in assemblies)
            {
                object attribute = assembly.GetCustomAttribute(customizationAttribute);
                if (attribute != null)
                {
                    var typeNameProperty = customizationAttribute.GetRuntimeProperty(RazorEngineCustomizationAttribute.FullTypeNameProperty);
                    var methodNameProperty = customizationAttribute.GetRuntimeProperty(RazorEngineCustomizationAttribute.MethodNameProperty);

                    var typeName = (string)typeNameProperty.GetValue(attribute);
                    var methodName = (string)methodNameProperty.GetValue(attribute);

                    var configureType = assembly.GetType(typeName);
                    var configureMethod = configureType.GetRuntimeMethod(methodName, configureMethodParameters);

                    configureMethods.Add(configureMethod);
                }
            }

            var razorEngineType = razorAssembly.GetType(RazorEngine.TypeFullName, throwOnError: true);
            var buildMethod = razorEngineType.GetRuntimeMethod(RazorEngine.CreateMethodName, configureMethodParameters);

            var configureThunkMethod = typeof(RazorEngineAdapter).GetTypeInfo().GetMethod(nameof(ConfigureEngine), BindingFlags.Static | BindingFlags.NonPublic);
            var thunk = configureThunkMethod.MakeGenericMethod(razorEngineBuilderType).Invoke(null, new object[] { configureMethods });

            var engine = buildMethod.Invoke(null, new object[] { thunk, });
            return new DefaultRazorEngineAdapter(razorEngineType, engine);
        }

        // Called by reflection
        private static Action<TBuilder> ConfigureEngine<TBuilder>(IEnumerable<MethodInfo> configureMethods)
        {
            return (TBuilder builder) =>
            {
                foreach (var configureMethod in configureMethods)
                {
                    configureMethod.Invoke(null, new object[] { builder, });
                }
            };
        }

        private class DefaultRazorEngineAdapter : RazorEngineAdapter
        {
            private object _razorEngine;
            private readonly Type _razorEngineType;

            public DefaultRazorEngineAdapter(Type razorEngineType, object razorEngine)
            {
                _razorEngineType = razorEngineType;
                _razorEngine = razorEngine;
            }
        }
    }
}
