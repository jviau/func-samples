// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Mono.Cecil;
using NuGet.Common;

namespace Azure.Functions.Sdk;

public sealed partial class FunctionsAssemblyScanner
{
    private const string FunctionAttribute = "Microsoft.Azure.Functions.Worker.FunctionAttribute";
    private const string BindingType = "Microsoft.Azure.Functions.Worker.Extensions.Abstractions.BindingAttribute";
    private const string OutputBindingType = "Microsoft.Azure.Functions.Worker.Extensions.Abstractions.OutputBindingAttribute";

    public IEnumerable<string> GetUsedWorkerAssemblies(string assembly, ILogger? logger = null)
    {
        AssemblyDefinition definition = ReadAssembly(assembly);
        logger ??= NullLogger.Instance;

        foreach (ModuleDefinition module in definition.Modules)
        {
            foreach (AssemblyDefinition assemblyDef in GetBindingSourceAssemblies(module, logger))
            {
                yield return assemblyDef.Name.FullName;
            }
        }
    }

    private static IEnumerable<AssemblyDefinition> GetBindingSourceAssemblies(ModuleDefinition module, ILogger logger)
    {
        foreach (TypeDefinition type in module.Types)
        {
            foreach (MethodDefinition method in type.Methods)
            {
                if (IsFunction(method))
                {
                    logger.LogDebug($"Found function: {method.Name} in {type.FullName}");
                    foreach (AssemblyDefinition assembly in GetBindingSourceAssemblies(method, logger))
                    {
                        logger.LogDebug($"Found binding source assembly: {assembly.Name.Name}");
                        yield return assembly;
                    }
                }
            }
        }
    }

    private static bool IsFunction(MethodDefinition method)
    {
        foreach (CustomAttribute attribute in method.CustomAttributes)
        {
            if (string.Equals(attribute.AttributeType.FullName, FunctionAttribute, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<AssemblyDefinition> GetBindingSourceAssemblies(MethodDefinition method, ILogger logger)
    {
        foreach (ParameterDefinition parameter in method.Parameters)
        {
            foreach (CustomAttribute attribute in parameter.CustomAttributes)
            {
                if (IsFunctionBindingType(attribute, logger))
                {
                    yield return attribute.AttributeType.Resolve().Module.Assembly;
                }
            }
        }
    }

    private static bool IsFunctionBindingType(CustomAttribute attribute, ILogger logger)
    {
        TypeReference? baseTypeRef = attribute.AttributeType?.Resolve()?.BaseType;
        if (baseTypeRef == null)
        {
            return false;
        }

        return baseTypeRef.CheckTypeInheritance(
            type => string.Equals(type.FullName, BindingType, StringComparison.Ordinal)
                || string.Equals(type.FullName, OutputBindingType, StringComparison.Ordinal),
            logger);
    }
}
