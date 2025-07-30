// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;

namespace Azure.Functions.Sdk.Tasks.Tests;

public class WriteExtensionProjectTests
{
    [Fact]
    public void Execute_Success()
    {
        const string outputPath = "obj/TestProj/azure_functions/azure_functions.gen.csproj";
        MockFileSystem fileSystem = new();
        fileSystem.AddEmptyFile(outputPath);

        WriteExtensionProject task = new(fileSystem, TimeProvider.System)
        {
            ExtensionPackages =
            [
                CreateTaskItem("Microsoft.Azure.Functions.Worker.Extensions.Http", "1.0.0"),
                CreateTaskItem("Microsoft.Azure.Functions.Worker.Extensions.Storage", "2.0.0")
            ],
            ProjectPath = outputPath,
            BuildEngine = Mock.Of<IBuildEngine>(),
        };

        task.Execute().Should().BeTrue();
    }

    private static TaskItem CreateTaskItem(string name, string version)
    {
        TaskItem item = new(name);
        item.SetVersion(version);
        return item;
    }
}
