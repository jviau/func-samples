// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Microsoft.Build.Framework;

namespace Azure.Functions.Sdk.Tasks.Extensions;

public class CalculateUnusedExtensionPackages(IFileSystem fileSystem)
    : Microsoft.Build.Utilities.Task, ICancelableTask, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly IFileSystem _fileSystem = Throw.IfNull(fileSystem);

    public CalculateUnusedExtensionPackages()
        : this(new FileSystem())
    {
    }

    [Required]
    public ITaskItem[] ExtensionPackages { get; set; } = [];

    [Required]
    public ITaskItem[] WorkerAssemblies { get; set; } = [];

    public string? CachePath { get; set; }

    [Output]
    public ITaskItem[] UnusedExtensionPackages { get; private set; } = [];

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
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
        }

        return !Log.HasLoggedErrors;
    }

    private void ExecuteCore()
    {
        UnusedExtensionPackages = [.. GetUnusedExtensionPackages()];
        if (!string.IsNullOrEmpty(CachePath))
        {
            Log.LogMessage(MessageImportance.Low, "Writing unused extension packages to cache: {0}", CachePath);
            List<string> unusedPackages = [.. UnusedExtensionPackages.Select(item => item.ItemSpec)];
            _fileSystem.File.WriteAllLines(CachePath!, unusedPackages);
        }
    }

    /// <summary>
    /// Calculates what extension packages are ultimately used based on the compilation result.
    /// "Used" here means that some binding from the WORKER package is used, and said worker package
    /// is the source of the extension package. This will err on the side of caution and include a
    /// package if any of the metadata to make the correct call is missing.
    /// </summary>
    /// <returns>The set of items from <see cref="ExtensionPackages"/> that are used.</returns>
    private IEnumerable<ITaskItem> GetUnusedExtensionPackages()
    {
        FunctionsAssemblyScanner scanner = FunctionsAssemblyScanner.FromTaskItems(WorkerAssemblies);
        HashSet<string> usedExtensions = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> assemblyToPackageMap = new(StringComparer.Ordinal);
        MSBuildNugetLogger logger = new(Log);
        foreach (ITaskItem item in WorkerAssemblies)
        {
            if (!ShouldScanWorkerAssembly(item))
            {
                continue;
            }

            _cts.Token.ThrowIfCancellationRequested();

            // If this assembly is part of a NuGet package, map it to the package ID.
            if (item.TryGetNuGetPackageId(out string? packageId))
            {
                assemblyToPackageMap[item.GetModuleName()] = packageId;
            }

            Log.LogMessage(MessageImportance.Low, "Scanning assembly: {0}", item.ItemSpec);
            try
            {
                foreach (string extension in scanner.GetUsedWorkerAssemblies(item.ItemSpec, logger))
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    usedExtensions.Add(extension);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("Error scanning assembly '{0}': {1}", item.ItemSpec, ex.Message);
                throw;
            }
        }

        // These are all the extension-containing nuget packages which are actually used.
        HashSet<string> usedWorkerPackages = new(
            usedExtensions.Select(extension => assemblyToPackageMap[extension]),
            StringComparer.OrdinalIgnoreCase);

        foreach (ITaskItem item in ExtensionPackages)
        {
            if (item.GetCanTrim()
                && item.TryGetSourcePackageId(out string? packageId)
                && !usedWorkerPackages.Contains(packageId))
            {
                // This item can be trimmed and is not used by the worker project.
                yield return item;
            }
        }
    }

    private bool ShouldScanWorkerAssembly(ITaskItem item)
    {
        return item.GetMetadata("FrameworkReferenceName") == string.Empty // framework references are not scanned.
            && (!item.TryGetNuGetPackageId(out string? packageId) // if it has a NugetPackageId, is it excluded?
                || !FunctionsAssemblyScanner.IsExcludedPackage(packageId));
    }
}
