// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AwesomeAssertions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;

namespace Azure.Functions.Sdk.Tasks.Extensions.Tests;

public class ValidateExtensionPackagesTests
{
    [Fact]
    public void Execute_Success()
    {
        // Arrange
        Mock<IBuildEngine> engine = new();
        engine.Setup(m => m.LogWarningEvent(It.IsAny<BuildWarningEventArgs>()))
              .Callback<BuildWarningEventArgs>(Callback);
        ValidateExtensionPackages task = new()
        {
            ExtensionPackages =
            [
                CreateTaskItem("Test.Package", "1.0.0"),
                CreateTaskItem("Test.Package", "1.0.0"),
            ],
            BuildEngine = engine.Object,
        };

        task.Execute().Should().BeTrue();
    }

    private static void Callback(BuildWarningEventArgs args)
    {
        Console.WriteLine($"Warning: {args.Message}");
    }

    private static TaskItem CreateTaskItem(string name, string version)
    {
        TaskItem item = new(name);
        item.SetVersion(version);
        return item;
    }
}
