// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AwesomeAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;

namespace Azure.Functions.Sdk.Tasks.Tests;

public class ResolveUsedExtensionPackagesTests
{
    [Fact]
    public void Execute_Success()
    {
        const string outputPath = "../../../obj/TestProj/azure_functions/obj";
        ResolveUsedExtensionPackages task = new()
        {
            RestoreOutputPath = outputPath,
            BuildEngine = Mock.Of<IBuildEngine>(),
            UnusedPackages =
            [
                new TaskItem("Microsoft.Azure.WebJobs.Extensions.Storage.Queues"),
            ],
        };

        task.Execute().Should().BeTrue();
    }
}
