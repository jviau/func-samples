// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure.Functions.Sdk.Tasks;
using NuGet.Common;

namespace Azure.Functions.Sdk;

/// <summary>
/// Used in conjunction with <see cref="MSBuildLogExtensions.LogCode"/>.
/// LogCodes must:
/// - Be defined in the 'Strings.resx' file.
/// - Name start with the log level followed by an underscore.
/// - Value start with the MSBuild log code prefix followed by a colon.
/// - The comment should have '{StrBegin="{Code}: "}' to indicate the start of the localizable log message.
/// Example: "Error_MyLogCode" with value "AZFW0100: My log message". and comment "{StrBegin="AZFW0100: "}".
/// </summary>
/// <param name="level">The level of the log message.</param>
/// <param name="id">The resource identifier for the log message.</param>
internal readonly struct LogCode(LogLevel level, string id)
{
    public static readonly LogCode Error_CannotRunFuncCli = new(nameof(Strings.Error_CannotRunFuncCli));

    public static readonly LogCode Error_ExtensionPackageConflict = new(nameof(Strings.Error_ExtensionPackageConflict));

    public static readonly LogCode Warning_ExtensionPackageDuplicate = new(nameof(Strings.Warning_ExtensionPackageDuplicate));

    public static readonly LogCode Error_InvalidExtensionPackageVersion = new(nameof(Strings.Error_InvalidExtensionPackageVersion));

    public static readonly LogCode Warning_EndOfLifeFunctionsVersion = new(nameof(Strings.Warning_EndOfLifeFunctionsVersion));

    public static readonly LogCode Error_UsingLegacyFunctionsSdk = new(nameof(Strings.Error_UsingLegacyFunctionsSdk));

    public static readonly LogCode Error_UnknownFunctionsVersion = new(nameof(Strings.Error_UnknownFunctionsVersion));

    public static readonly LogCode Error_UnsupportedTargetFramework = new(nameof(Strings.Error_UnsupportedTargetFramework));

    public static readonly LogCode Error_CustomFunctionPackageReferencesNotAllowed = new(nameof(Strings.Error_CustomFunctionPackageReferencesNotAllowed));

    public LogCode(string id)
        : this(ParseLevel(id), id)
    {
    }

    public LogLevel Level => level;

    public string Id => id;

    /// <summary>
    /// Gets a <see cref="LogCode"/> from its identifier.
    /// </summary>
    /// <param name="id">The resource identifier.</param>
    /// <returns>The <see cref="LogCode"/>.</returns>
    public static LogCode FromId(string id)
    {
        Throw.IfNullOrEmpty(id);
        return id switch
        {
            nameof(Error_CannotRunFuncCli) => Error_CannotRunFuncCli,
            nameof(Error_ExtensionPackageConflict) => Error_ExtensionPackageConflict,
            nameof(Warning_ExtensionPackageDuplicate) => Warning_ExtensionPackageDuplicate,
            nameof(Error_InvalidExtensionPackageVersion) => Error_InvalidExtensionPackageVersion,
            nameof(Warning_EndOfLifeFunctionsVersion) => Warning_EndOfLifeFunctionsVersion,
            nameof(Error_UsingLegacyFunctionsSdk) => Error_UsingLegacyFunctionsSdk,
            nameof(Error_UnknownFunctionsVersion) => Error_UnknownFunctionsVersion,
            nameof(Error_UnsupportedTargetFramework) => Error_UnsupportedTargetFramework,
            nameof(Error_CustomFunctionPackageReferencesNotAllowed) => Error_CustomFunctionPackageReferencesNotAllowed,
            _ => new LogCode(id),
        };
    }

    private static LogLevel ParseLevel(string id)
    {
        int index = id.IndexOf('_');
        if (index < 0)
        {
            throw new ArgumentException($"Unable to determine log level from: {id}. Id must start with {{LogLevel}}_", nameof(id));
        }

        string levelString = id[..index];
        return !Enum.TryParse(levelString, out LogLevel level)
            ? throw new ArgumentException($"Unknown log level: {levelString}", nameof(id))
            : level;
    }
}
