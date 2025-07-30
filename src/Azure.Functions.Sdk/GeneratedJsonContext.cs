// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Azure.Functions.Sdk;

[JsonSourceGenerationOptions(
        GenerationMode = JsonSourceGenerationMode.Serialization,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        WriteIndented = true)]
[JsonSerializable(typeof(WebJobsExtensions))]
public partial class GeneratedJsonContext : JsonSerializerContext
{
}
