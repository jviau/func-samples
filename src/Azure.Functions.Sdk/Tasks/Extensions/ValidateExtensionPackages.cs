// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Build.Framework;

namespace Azure.Functions.Sdk.Tasks.Extensions;

public class ValidateExtensionPackages : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[] ExtensionPackages { get; set; } = [];

    [Output]
    public ITaskItem[] FilteredPackages { get; private set; } = [];

    public override bool Execute()
    {
        // key=identity, value=version
        Dictionary<string, ITaskItem> uniquePackages = new(StringComparer.OrdinalIgnoreCase);
        foreach (ITaskItem package in ExtensionPackages)
        {
            if (uniquePackages.TryGetValue(package.ItemSpec, out ITaskItem? existingPackage))
            {
                bool existingImplicit = existingPackage.GetIsImplicitlyDefined();
                bool newImplicit = package.GetIsImplicitlyDefined();
                if (existingImplicit)
                {
                    // If existing is implicit, always replace. Event if new is implicit.
                    uniquePackages[package.ItemSpec] = package;
                    continue;
                }
                if (newImplicit && !existingImplicit)
                {
                    // If existing is not implicit, and new is implicit, skip the implicit.
                    continue;
                }

                // Both are explicit packages. If the package already exists:
                //  Log a warning and continue if versions match.
                //  Log an error if versions do not match.
                string version = existingPackage.GetVersion();
                string newVersion = package.GetVersion();
                if (version != newVersion)
                {
                    Log.LogCode(LogCode.ExtensionPackageConflict, package.ItemSpec, version, newVersion);
                }
                else
                {
                    Log.LogCode(LogCode.ExtensionPackageDuplicate, package.ItemSpec, version);
                }
            }
            else
            {
                // Check version is a valid nuget package version.
                string version = package.GetVersion();
                if (!NuGet.Versioning.NuGetVersion.TryParse(version, out _))
                {
                    Log.LogCode(LogCode.InvalidExtensionPackageVersion, package.ItemSpec, version);
                }

                uniquePackages[package.ItemSpec] = package;
            }
        }

        FilteredPackages = [.. uniquePackages.Values];
        return !Log.HasLoggedErrors;
    }
}
