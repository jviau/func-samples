// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AwesomeAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;

namespace Azure.Functions.Sdk.Tasks.Extensions.Tests;

public class CalculateUnusedExtensionReferencesTests
{
    [Fact]
    public void Execute_Success()
    {
        // Arrange
        TaskItem serviceBus = new(@"C:\Users\javia\.nuget\packages\microsoft.azure.functions.worker.extensions.servicebus\5.22.0\lib\netstandard2.0\Microsoft.Azure.Functions.Worker.Extensions.ServiceBus.dll");
        serviceBus.SetMetadata("NuGetPackageId", "Microsoft.Azure.Functions.Worker.Extensions.ServiceBus");
        serviceBus.SetMetadata("FusionName", "Microsoft.Azure.Functions.Worker.Extensions.ServiceBus, Version=5.22.0.0, Culture=neutral, PublicKeyToken=551316b6919f366c");

        TaskItem queues = new(@"C:\Users\javia\.nuget\packages\microsoft.azure.functions.worker.extensions.storage.queues\5.22.0\lib\netstandard2.0\Microsoft.Azure.Functions.Worker.Extensions.StorageQueues.dll");

        TaskItem[] workerAssemblies =
        [
            new TaskItem(@"C:\repos\test\sdk\out\bin\TestProj\debug\TestProj.dll"),
            serviceBus,
        ];

        GenerateWebJobsMetadata task = new()
        {
            OutputPath = "extensions.json",
            BuildEngine = Mock.Of<IBuildEngine>(),
        };

        task.Execute().Should().BeTrue();
    }
}
