// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Build.Framework;

namespace Azure.Functions.Sdk.Tasks.Inner;

public class ResolveExtensionCopyLocal : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[] UsedPackages { get; set; } = [];

    [Required]
    public ITaskItem[] RuntimeAssemblies { get; set; } = [];

    [Required]
    public ITaskItem[] CopyLocalFiles { get; set; } = [];

    [Output]
    public ITaskItem[] ExtensionsCopyLocal { get; private set; } = [];

    public override bool Execute()
    {
        HashSet<string> usedPackageIds = new(UsedPackages.Select(p => p.ItemSpec), StringComparer.OrdinalIgnoreCase);
        HashSet<string> runtimeAssemblyNames = new(RuntimeAssemblies.Select(p => p.ItemSpec), StringComparer.OrdinalIgnoreCase);

        List<ITaskItem> extensionsCopyLocal = [];
        foreach (ITaskItem item in CopyLocalFiles)
        {
            if (item.TryGetNuGetPackageId(out string? packageId)
                && usedPackageIds.Contains(packageId)
                && !runtimeAssemblyNames.Contains(Path.GetFileName(item.ItemSpec)))
            {
                string destination = item.GetMetadata("DestinationSubPath");
                item.SetMetadata("TargetPath", Path.Combine(Constants.ExtensionsOutputFolder, destination));
                extensionsCopyLocal.Add(item);
            }
        }

        ExtensionsCopyLocal = [.. extensionsCopyLocal];
        return !Log.HasLoggedErrors;
    }
}
