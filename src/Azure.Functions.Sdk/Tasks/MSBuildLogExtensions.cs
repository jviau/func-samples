// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Common;

namespace Azure.Functions.Sdk.Tasks;

internal static class MSBuildLogExtensions
{
    public static void LogCode(this TaskLoggingHelper log, LogCode logCode)
        => LogCode(log, logCode, []);

    public static void LogCode(this TaskLoggingHelper log, LogCode logCode, params string[] messageArgs)
    {
        log.HelpKeywordPrefix = $"AzureFunctions";
        log.TaskResources = Strings.ResourceManager;
        switch (logCode.Level)
        {
            case LogLevel.Error:
                log.LogErrorWithCodeFromResources(logCode.Id, messageArgs);
                break;
            case LogLevel.Warning:
                log.LogWarningWithCodeFromResources(logCode.Id, messageArgs);
                break;
            case LogLevel.Information:
                log.LogMessageFromResources(MessageImportance.High, logCode.Id, messageArgs);
                break;
            case LogLevel.Debug:
                log.LogMessageFromResources(MessageImportance.Low, logCode.Id, messageArgs);
                break;
            case LogLevel.Minimal:
                log.LogMessageFromResources(MessageImportance.Normal, logCode.Id, messageArgs);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(logCode.Level), logCode.Level, "Unsupported log level");
        }
    }
}
