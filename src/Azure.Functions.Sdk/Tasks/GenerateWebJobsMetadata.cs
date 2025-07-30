// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;

namespace Azure.Functions.Sdk.Tasks;

public partial class GenerateWebJobsMetadata(IFileSystem fileSystem)
    : Microsoft.Build.Utilities.Task, ICancelableTask, IDisposable
{
    private static readonly Regex ExcludedWorkerAssembliesRegex = new(
        @"^(System|Azure\.Core|Azure\.Identity|Microsoft\.Bcl|Microsoft\.Extensions|Microsoft\.Identity|Microsoft\.NETCore|Microsoft\.NETStandard|Microsoft\.Win32|Grpc)\..*",
        RegexOptions.Compiled);

    private static readonly HashSet<string> ExcludedAssemblies = new(StringComparer.OrdinalIgnoreCase)
    {
        "Microsoft.Azure.WebJobs.Extensions.dll",
        "Microsoft.Azure.WebJobs.Extensions.Http.dll",
    };

    private readonly CancellationTokenSource _cts = new();
    private readonly IFileSystem _fileSystem = Throw.IfNull(fileSystem);

    public GenerateWebJobsMetadata()
        : this(new FileSystem())
    {
    }

    [Required]
    public string OutputPath { get; set; } = string.Empty;

    [Required]
    public ITaskItem[] ExtensionReferences { get; set; } = [];

    public void Cancel()
    {
        _cts.Cancel();
    }

    public void Dispose()
    {
        _cts.Dispose();
    }

    public override bool Execute()
    {
        try
        {
            ExecuteCore();
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
        }

        return !Log.HasLoggedErrors;
    }

    private void ExecuteCore()
    {
        List<WebJobsReference> references = [];
        MSBuildNugetLogger logger = new(Log);

        FunctionsAssemblyScanner scanner = new();
        foreach (ITaskItem item in GetAssembliesToScan())
        {
            _cts.Token.ThrowIfCancellationRequested();

            try
            {
                references.AddRange(scanner.GetWebJobsReferences(item.ItemSpec, logger));
            }
            catch (Exception ex)
            {
                Log.LogError("Error scanning assembly '{0}': {1}", item.ItemSpec, ex.Message);
                throw;
            }
        }

        WebJobsExtensions extensions = new(references);
        string json = JsonSerializer.Serialize(extensions, GeneratedJsonContext.Default.WebJobsExtensions);
        _cts.Token.ThrowIfCancellationRequested();
        _fileSystem.File.WriteAllText(OutputPath, json);
    }

    private bool ShouldScanExtensionAssembly(ITaskItem item)
    {
        string fileName = Path.GetFileName(item.ItemSpec);
        return item.GetMetadata("FrameworkReferenceName") == string.Empty // framework references are not scanned.
            && !ExcludedAssemblies.Contains(fileName) // exclude known assemblies
            && (!item.TryGetNuGetPackageId(out string? packageId) // if it has a NugetPackageId, is it excluded?
                || !FunctionsAssemblyScanner.IsExcludedPackage(packageId));
    }

    private IEnumerable<ITaskItem> GetAssembliesToScan()
    {
        foreach (ITaskItem item in ExtensionReferences)
        {
            if (!ShouldScanExtensionAssembly(item))
            {
                Log.LogMessage(
                    MessageImportance.Low,
                    "Skipping extension assembly '{0}' as it is explicitly excluded.",
                    item.ItemSpec);
                continue;
            }

            yield return item;
        }
    }
}
