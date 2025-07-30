// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Build.Framework;

namespace Azure.Functions.Sdk.Tasks;

public class ValidateExtensionPackages : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[] ExtensionPackages { get; set; } = [];

    [Output]
    public ITaskItem[] FilteredPackages { get; private set; } = [];

    public override bool Execute()
    {
        bool success = true;

        // key=identity, value=version
        Dictionary<string, ITaskItem> uniquePackages = new(StringComparer.OrdinalIgnoreCase);

        foreach (ITaskItem package in ExtensionPackages)
        {
            if (uniquePackages.TryGetValue(package.ItemSpec, out ITaskItem? existingPackage))
            {
                // If the package already exists, check if the version is the same
                string version = existingPackage.GetMetadata("Version");
                string newVersion = package.GetMetadata("Version");
                if (version != newVersion)
                {
                    // TODO: group up duplicates and emit a single error at the end.
                    success = false;
                    Log.LogError(
                        "Duplicate extension package found with different versions: {0}/{1} and {0}/{2}",
                        package.ItemSpec,
                        version,
                        newVersion);
                }
                else
                {
                    Log.LogMessage(
                        MessageImportance.Low,
                        "Ignoring duplicate extension package found: {0}/{1}",
                        package.ItemSpec,
                        version);
                }
            }
            else
            {
                // Validate version is a valid nuget package version.
                string version = package.GetMetadata("Version");
                if (!NuGet.Versioning.NuGetVersion.TryParse(version, out _))
                {
                    success = false;
                    Log.LogError(
                        "Invalid version for extension package {0}: {1}. Version must be a valid NuGet version.",
                        package.ItemSpec,
                        version);
                }

                uniquePackages[package.ItemSpec] = package;
            }
        }

        if (!success)
        {
            Log.LogError("Validation of extension packages failed.");
            return false;
        }

        Log.LogMessage(MessageImportance.Low, "Validated {0} extension packages.", uniquePackages.Count);
        FilteredPackages = [.. uniquePackages.Values];
        return success;
    }
}
