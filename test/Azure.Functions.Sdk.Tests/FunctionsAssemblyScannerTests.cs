// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AwesomeAssertions;

namespace Azure.Functions.Sdk.Tests;

public class FunctionsAssemblyScannerTests
{
    [Fact]
    public void ScanForWebJobsReferences_Succeeds()
    {
        const string path = @"C:\Users\javia\.nuget\packages\microsoft.azure.webjobs.extensions.servicebus\5.13.5\lib\net6.0\Microsoft.Azure.WebJobs.Extensions.ServiceBus.dll";
        FunctionsAssemblyScanner scanner = new();
        IEnumerable<WebJobsReference> results = scanner.GetWebJobsReferences(path);
        results.Should().HaveCount(1);
    }
}
