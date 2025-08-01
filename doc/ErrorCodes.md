# Error Codes

## AZFW0100

| Item | Value |
| - | - |
| Name | ErrorRunningFuncCli |
| Severity | Error |
| Source | MSBuild targets |
| HelpLink | https://aka.ms/azfunc-dotnet-run-error |

Unable to launch `func` cli when performing `dotnet run` on a function app project.

**Recommended Action**: Ensure `func` cli is installed and part of the PATH. See https://aka.ms/azfunc-dotnet-run-error.

## AZFW0101

| Item | Value |
| - | - |
| Name | ExtensionPackageConflict |
| Severity | Error |
| Source | MSBuild targets |
| HelpLink | N/A |

The item group `AzureFunctionsPackageExtension` has duplicate items with conflicting versions.

**Recommended Action**: Remove the conflicting `AzureFunctionsPackageExtension` item(s).

## AZFW0102

| Item | Value |
| - | - |
| Name | ExtensionPackageDuplicate |
| Severity | Warning |
| Source | MSBuild targets |
| HelpLink | N/A |

The item group `AzureFunctionsPackageExtension` has duplicate items with identical versions.

**Recommended Action**: Remove the duplicate `AzureFunctionsPackageExtension` item(s).

## AZFW0103

| Item | Value |
| - | - |
| Name | InvalidExtensionPackageVersion |
| Severity | Error |
| Source | MSBuild targets |
| HelpLink | N/A |

The item group `AzureFunctionsPackageExtension` has an item with an invalid `Version`.

**Recommended Action**: Provide a valid `Version` to the identified packages.

## AZFW0104

| Item | Value |
| - | - |
| Name | EndOfLifeFunctionsVersion |
| Severity | Warning |
| Source | MSBuild targets |
| HelpLink | https://aka.ms/azure-functions-retired-versions |

Azure Functions '[VERSION]' is out of support.

**Recommended Action**: Update `AzureFunctionsVersion` property to an in-support value.

## AZFW0105

| Item | Value |
| - | - |
| Name | UsingLegacyFunctionsSdk |
| Severity | Error |
| Source | MSBuild targets |
| HelpLink | N/A |

Reference 'Microsoft.NET.Sdk.Functions' from a dotnet isolated function application.

**Recommended Action**: Remove `Microsoft.NET.Sdk.Functions` reference.

## AZFW0106

| Item | Value |
| - | - |
| Name | UnknownFunctionsVersion |
| Severity | Error |
| Source | MSBuild targets |
| HelpLink | N/A |

AzureFunctionsVersion '[VERSION]' is unknown or out of support.

**Recommended Action**: Update `AzureFunctionsVersion` property to a supported value.
