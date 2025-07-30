// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.LibraryModel;
using NuGet.Packaging.Core;
using NuGet.ProjectModel;

namespace Azure.Functions.Sdk.Tasks;

public class ResolveUsedExtensionPackages(IFileSystem fileSystem) : Microsoft.Build.Utilities.Task
{
    private readonly IFileSystem _fileSystem = Throw.IfNull(fileSystem);

    public ResolveUsedExtensionPackages()
        : this(new FileSystem())
    {
    }

    [Required]
    public string RestoreOutputPath { get; set; } = string.Empty;

    [Required]
    public ITaskItem[] UnusedPackages { get; set; } = [];

    [Output]
    public ITaskItem[] UsedPackages { get; private set; } = [];

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
        if (!TryGetLockFile(out LockFile? lockFile))
        {
            return;
        }

        if (lockFile.Targets.Count > 1)
        {
            Log.LogError("Multiple targets found in lock file. This project should only have 1 TFM.");
            return;
        }

        LockFileTarget target = lockFile.Targets[0];
        if (!string.IsNullOrEmpty(target.RuntimeIdentifier))
        {
            Log.LogError("Lock file contains a runtime identifier. This project should not have a runtime identifier.");
            return;
        }

        Dictionary<string, LockFileTargetLibrary> skipped = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> usedPackages = GetTopLevelUsedPackages(lockFile);
        List<ITaskItem> references = [];
        foreach (LockFileTargetLibrary library in target.Libraries)
        {
            if (!ShouldIncludePackage(library, out string? name))
            {
                continue;
            }

            if (!usedPackages.Contains(name))
            {
                skipped[name] = library;
            }
            else
            {
                references.AddRange(IncludePackage(library, usedPackages, skipped));
            }
        }

        UsedPackages = [.. references];
    }

    private bool TryGetLockFile([NotNullWhen(true)] out LockFile? lockFile)
    {
        string assetsFile = Path.Combine(RestoreOutputPath, LockFileFormat.AssetsFileName);
        if (!_fileSystem.File.Exists(assetsFile))
        {
            Log.LogError("Assets file '{0}' does not exist. Please ensure restore successfully ran.", assetsFile);
            lockFile = null;
            return false;
        }

        IFileInfo info = _fileSystem.FileInfo.New(assetsFile);
        using FileSystemStream stream = info.OpenRead();
        LockFileFormat format = new();
        lockFile = format.Read(stream, new MSBuildNugetLogger(Log), assetsFile);
        return true;
    }

    /// <summary>
    /// Gets the packages that are directly referenced by the project, excluding any known unused packages.
    /// </summary>
    /// <param name="lockFile">The lockfile to resolve packages from.</param>
    /// <returns>The set of used packages.</returns>
    private HashSet<string> GetTopLevelUsedPackages(LockFile lockFile)
    {
        HashSet<string> unusedPackages = new(UnusedPackages.Select(x => x.ItemSpec), StringComparer.OrdinalIgnoreCase);
        HashSet<string> usedPackages = new(StringComparer.OrdinalIgnoreCase);

        // already verified 1 TFM, this should still hold true.
        ProjectFileDependencyGroup dep = lockFile.ProjectFileDependencyGroups.Single();
        foreach (string item in dep.Dependencies)
        {
            string name = item.Split(' ')[0];
            if (!unusedPackages.Contains(name))
            {
                usedPackages.Add(name);
            }
        }

        return usedPackages;
    }

    private IEnumerable<ITaskItem> IncludePackage(
        LockFileTargetLibrary library,
        HashSet<string> usedPackages,
        Dictionary<string, LockFileTargetLibrary> skipped)
    {
        if (!ShouldIncludePackage(library, out _))
        {
            yield break;
        }

        usedPackages.Add(library.Name!);
        yield return new TaskItem(library.Name!);

        foreach (PackageDependency dep in library.Dependencies)
        {
            // A previously skipped package now needs to be included as it is used.
            if (skipped.TryGetValue(dep.Id, out LockFileTargetLibrary? skippedLibrary))
            {
                skipped.Remove(dep.Id);
                foreach (ITaskItem item in IncludePackage(skippedLibrary, usedPackages, skipped))
                {
                    yield return item;
                }
            }
            else
            {
                // Add this so it will be included when we encounter it.
                usedPackages.Add(dep.Id);
            }
        }
    }

    private bool ShouldIncludePackage(LockFileTargetLibrary library, [NotNullWhen(true)] out string? name)
    {
        name = library.Name;
        return library.Type == LibraryType.Package && !string.IsNullOrEmpty(name);
    }
}
