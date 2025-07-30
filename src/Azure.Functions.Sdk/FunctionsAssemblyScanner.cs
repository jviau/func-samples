// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Mono.Cecil;
using NuGet.Common;

namespace Azure.Functions.Sdk;

public sealed partial class FunctionsAssemblyScanner
{
    private static readonly Regex ExcludedPackagesRegex = new(
        @"^(System|Azure\.Core|Azure\.Identity|Microsoft\.Bcl|Microsoft\.Extensions|Microsoft\.Identity|Microsoft\.NETCore|Microsoft\.NETStandard|Microsoft\.Win32|Grpc)\..*",
        RegexOptions.Compiled);

    private readonly FunctionsAssemblyResolver _resolver;
    private readonly ReaderParameters _readerParameters;

    public FunctionsAssemblyScanner()
    {
        _resolver = new();
        _readerParameters = new ReaderParameters
        {
            AssemblyResolver = _resolver,
        };
    }

    public static bool IsExcludedPackage(string name)
    {
        return string.IsNullOrEmpty(name) || ExcludedPackagesRegex.IsMatch(name);
    }

    public IEnumerable<WebJobsReference> GetWebJobsReferences(string assembly, ILogger? logger = null)
    {
        AssemblyDefinition definition = ReadAssembly(assembly);
        return WebJobsReference.FromModule(definition, logger);
    }

    private AssemblyDefinition ReadAssembly(string assemblyPath)
    {
        Throw.IfNullOrEmpty(assemblyPath);
        _resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
        return AssemblyDefinition.ReadAssembly(assemblyPath, _readerParameters);
    }
}
