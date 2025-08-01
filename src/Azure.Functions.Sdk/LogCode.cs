// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure.Functions.Sdk.Tasks;
using NuGet.Common;

namespace Azure.Functions.Sdk;

/// <summary>
/// Used in conjunction with <see cref="MSBuildLogExtensions.LogCode"/>.
/// </summary>
/// <param name="level">The level of the log message.</param>
/// <param name="id">The resource identifier for the log message.</param>
internal readonly struct LogCode(LogLevel level, string id)
{
    public static LogCode ErrorRunningFuncCli => new(LogLevel.Error, nameof(ErrorRunningFuncCli));

    public static LogCode ExtensionPackageConflict => new(LogLevel.Error, nameof(ExtensionPackageConflict));

    public static LogCode ExtensionPackageDuplicate => new(LogLevel.Warning, nameof(ExtensionPackageDuplicate));

    public static LogCode InvalidExtensionPackageVersion => new(LogLevel.Error, nameof(InvalidExtensionPackageVersion));

    public static LogCode EndOfLifeFunctionsVersion => new(LogLevel.Warning, nameof(EndOfLifeFunctionsVersion));

    public static LogCode UsingLegacyFunctionsSdk => new(LogLevel.Error, nameof(UsingLegacyFunctionsSdk));

    public static LogCode UnknownFunctionsVersion => new(LogLevel.Error, nameof(UnknownFunctionsVersion));

    public static LogCode UnsupportedTargetFramework => new(LogLevel.Error, nameof(UnsupportedTargetFramework));

    public LogLevel Level => level;

    public string Id => id;

    /// <summary>
    /// Gets a <see cref="LogCode"/> from its identifier.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <returns>The <see cref="LogCode"/>.</returns>
    /// <exception cref="ArgumentException">If <paramref name="id"/> is not recognized.</exception>
    public static LogCode FromId(string id)
    {
        Throw.IfNullOrEmpty(id);
        return id switch
        {
            nameof(ErrorRunningFuncCli) => ErrorRunningFuncCli,
            nameof(ExtensionPackageConflict) => ExtensionPackageConflict,
            nameof(ExtensionPackageDuplicate) => ExtensionPackageDuplicate,
            nameof(InvalidExtensionPackageVersion) => InvalidExtensionPackageVersion,
            nameof(EndOfLifeFunctionsVersion) => EndOfLifeFunctionsVersion,
            nameof(UsingLegacyFunctionsSdk) => UsingLegacyFunctionsSdk,
            nameof(UnknownFunctionsVersion) => UnknownFunctionsVersion,
            nameof(UnsupportedTargetFramework) => UnsupportedTargetFramework,
            _ => throw new ArgumentException($"Unknown log code: {id}", nameof(id))
        };
    }
}
