$ErrorActionPreference = "Stop"
$path = Join-Path $PSScriptRoot "../.."
$projectPath = Join-Path $path "src/Azure.Functions.Sdk/Azure.Functions.Sdk.csproj"

dotnet pack $projectPath

$version = dotnet build $projectPath -getProperty:Version

$packagePath = Join-Path $path "out/pkg/release/Azure.Functions.Sdk.$version.nupkg"
Install-Nuget $packagePath -Force
