# Azure.Functions.Sdk Prototype

This branch demonstrates how we could rewrite our `Microsoft.Azure.Functions.Worker.Sdk` as an MSBuild SDK.

## Existing vs New

What do we have today, and why is an "MSBuild" SDK any different?

Today: we have a regular nuget package which is used via the `PackageReference` item group and is part of restore phase. NuGet hooks up our targets for us and customers get our targets at build time.
[MSBuild SDK](https://learn.microsoft.com/visualstudio/msbuild/how-to-use-project-sdk?view=vs-2022) This is the `Sdk=Microsoft.NET.Sdk` element! MSBuild SDKs can be resolved multiple ways, one of which is nuget packages! So we will remain as a nuget package and ship via nuget.org. But the primary benefit is we are resolved and installed **BEFORE** restore. This means our targets can influence restore!

Pros:
- Still a nuget package
- Much greater target control
  - We control when `Microsoft.NET.Sdk` is brought in.
- Can influence restore

Cons:
- Outside of the effort to migrate, none I can really think of. Customers may have a slight disruption shifting from `PackageReference` to `Sdk`, but that is very minor in my opinion.

## Why?

Why do this migration? Today the inner build, which we use to resolve the webjobs extensions payload, is problematic for customers for a few reasons. The primary reason is network isolation and that we perform a restore during build. Due to our targets today only being present _AFTER_ restore completes, there is next to nothing we can do to influence restore behavior. With an MSBuild SDK we have options, and this repo demonstrates that. We now generate and restore the inner build project as part of the outer restore phase!

## Behavior

### Restore Sequence

1. `Restore`
2. (after restore) `ResolveExtensionPackages`
   1. Loads `project.assets.json` (via `NuGet.ProjectModel`)
   2. Scans all reference assemblies from packages, looking for `ExtensionInformationAttribute` (via `Mono.Cecil`)
      1. Skips packages that are known to never contain an extension
   3. Returns all found webjobs extensions
3. `WriteExtensionProject`
   1. Calculates a hash of included webjobs extensions
   2. Compares with existing on-disk hash, if any
   3. Writes `azure_functions.g.csproj` if hash does not match
4. `RestoreExtensionProject`
   1. Invokes `Restore` target on `azure_functions.g.csproj`

### Build Sequence

1. `CoreCompile`
2. (after CoreCompile) `ResolveExtensionPackages`
   1. Same target as from restore sequence.
3. `_CalculateUnusedExtensionPackages`
   1. Scans all referenced assemblies, looking for binding usage.
   2. Identifies worker packages which are NOT used, then maps that to WebJobs packages which can be trimmed.
4. `GetFunctionsExtensionFiles`
   1. Invokes `GetFunctionsExtensionFiles` on `azure_functions.g.csproj`, passing in unused extensions
      1. `_ResolveUsedExtensionPackages`
         1. Identifies what WebJobs packages are used **AND** all necessary transitive package references
         2. Removes known "runtime" packages (packages we know the host controls loading of the contained assemblies)
      2. `ResolveReferences`
         1. Resolves all copy-local assemblies
      3. `GenerateBuildDependencyFile`
         1. Generates the deps.json file, this will become `function.deps.json`
      4. `GetCopyToOutputDirectoryItems`
         1. Collects any additional files to copy to output directory (incase an extension has extra content)
      5. `GenerateWebJobsMetadata`
      6. Finally aggregates all files, assign `TargetPath` to them, and returns to outer build
5. `GenerateWebJobsMetadata`
   1. Scans `runtime` files (assemblies) from inner build looking for `WebJobsStartupAttribute`
   2. Generates `extensions.json`, with hint path added.
6. `AssignFunctionsTargetPaths`
   1. Assigns `TargetPath` to the `extension.json` item
   2. Adds all extension files to `_NoneWithTargetPath` item group
7. `GetCopyToOutputDirectoryItems`
   1. Will now include all extension files, meaning when `CopyFilesToOutputDirectory` or `CopyFilesToPublishDirectory` run, they will copy over our SDK's output as well.

#### Sequence Diagram

Legend:
- Enclosed in `[]`: Existing target from `Microsoft.NET.Sdk` (these are also marked with red outline or red background)
- Blue background: target ran in inner project

``` mermaid
gantt
    title Azure.Functions.Sdk
    dateFormat X
    axisFormat %S
    section Restore
      [Restore]                          :crit, restore, 0, 100s
      GenerateFunctionsExtensionProject  :after restore, 120s
      ResolveExtensionPackages           :resolve, after restore, 80s
      WriteExtensionProject              :write, after resolve, 10s
      RestoreExtensionProject            :inner-restore, after write, 30s
      [Restore] inner                      :crit, active, after write, 30s
    section Build
      [CoreCompile]                      :crit, core, 0, 150s
      PrepareFunctionsExtensionPayload   :310s
      ResolveExtensionPackages           :resolve, after core, 80s
      _CalculateUnusedExtensionPackages  :calc, after resolve, 80s
      GetFunctionsExtensionFiles         :outer-get, after calc, 100s
      GetFunctionsExtensionFiles inner   :active, inner-get, after calc, 100s
      _ResolveUsedExtensionPackages      :active, used, after calc, 20s
      [ResolveReferences]                :crit, active, inner-resolve, after used, 40s
      [GenerateBuildDependencyFile]      :crit, active, deps, after inner-resolve, 20s
      [GetCopyToOutputDirectoryItems]    :crit, active, after deps, 10s
      GenerateWebJobsMetadata            :gen, after inner-get, 40s
      AssignFunctionsTargetPaths         :assign, after gen, 10s
      [GetCopyToOutputDirectoryItems]    :crit, copy, after assign, 10s
```


## Drawbacks

### Inconsistent Post-Restore Hook Support
Hooking into post-restore is inconsistent depending on how the function csproj is restored. We are seeing if dotnet/nuget/msbuild team is open to making all CLI scenarios work.

| Restore Method | Result |
| - | - |
| `dotnet restore MyFunctionApp.csproj` | post-restore hook works |
| `dotnet restore MySolution.sln` | Post-restore hook does not run. Customer will need a `Directory.Solution.targets` which calls our post restore hook. |
| `dotnet restore dirs.proj` | Post-restore hook does not run. Customer will need to add a target to dirs.proj file to call our post restore hook. |
| `dotnet restore SomeAppReferencingTheFunctionApp.csproj` | Post-restore hook does not run. Customer will need to manually call our post restore hook. |
| Restore in Visual Studio | Post restore hook does not run. VS restore is _very_ different from CLI restore, we may not be able to support this. We will fallback to restoring during build. |

### Complicated Trimming of Unused Extensions

Today we generated and restore the inner csproj _after_ compilation, so we know exactly what extensions the app does and does not use. We then exclude unused extensions from the inner project to slim down the payload and extension loads at runtime. With the msbuild SDK approach we generate the inner csproj before compilation, so we have no way of knowing what is and isn't used. Instead, we perform trimming by evaluating unused packages post-compilation and then walking the restore graph ourselves and removing assemblies from packages that are ultimately unused by the app. This works, but it has a very minor side effect: unused extensions may still affect the package versions of used extensions and/or their transitive references.

## Refactoring Wins

This refactor has also had some decent wins with improving the entire inner build loop:

1. We no longer use `Microsoft.NET.Sdk.Functions` at all. Instead, the inner project also uses `Azure.Functions.Sdk`. During evaluation of the inner project, the SDK will detect it is the generated project (recognized by name "azure_functions.g.csproj") and shift its import graph.
2. We no longer even _build_ the inner project. Instead, we call a target `ResolveFunctionsExtensionFiles` on it which will return the exact set of files to include in the `.azurefunctions` folder (and the `function.deps.json`).
   - This means we have less file copying. We copy directly from the nuget package location on disk instead of the inner projects build output.
3. Generating of `extensions.json` is now done in the outer project, scanning the resolved extension files directly.
4. Modifying inner-build packages now possible via msbuild items.
   - Targets can add or remove from the `AzureFunctionPackageReference` item group to manually control the inner project.

## Additional Opportunities / Open Questions

1. Moving analyzers/generators into the SDK.
   - These don't need to ship as separate nuget packages. We can directly include them in the SDK and add them as roslyn analyzers directly.
   - But we can keep them separate if desired. If so, we will move all analyzer/generator props/targets out into their own files that are part of those nuget packages directly.
2. Renaming: this prototype renames the SDK to `Azure.Functions.Sdk`. Why? Conciseness and alignment with Azure and Dotnet team.
   - `Sdk=Microsoft.Azure.Functions.Worker.Sdk/{Version}` is a bit verbose in my opinion. I much prefer `Sdk=Azure.Functions.Sdk/{Version}`
   - The Azure and Dotnet team have moved to using more concise package names. `Azure.*`, `Aspire.*`, etc, I believe we should follow suit. This is a perfect opportunity to do that _for the SDK_ at least. We can consider moving other assemblies later, if at all.
3. Have customers manually reference `Microsoft.Azure.Functions.Worker` or add it implicitly?
   - Our SDK can technically have a `<PackageReference Include="Microsoft.Azure.Functions.Worker" Version="[LATESTVERSION]" IsImplicitlyDefined="true" />` defined in its targets.
   - The `IsImplicitlyDefined="true"` is the secret sauce. This package ref will be dropped silently if a customer ever defines it themselves.
   - This ultimately makes a customer manually including our packages _optional_.
4. `ExtensionInformationAttribute` alternative. Since we are walking `project.assets.json`, which tells us all files a nuget package have, we could design a convention where worker packages could add an `azure_functions.extensions.json` or similar file, which we will read and add extensions from. This will be significantly more performant than scanning assemblies.
5. Lack of multi-targeting support. This prototype does not support multi-TFM's yet. We currently generate only 1 `azure_functions.g.csproj` for all TFM's. This currently cannot handle divergences in the set of extensions between TFMs.
