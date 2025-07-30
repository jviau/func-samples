// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Build.Framework;
using Moq;

namespace Azure.Functions.Sdk.Tasks.Tests;

public class ResolveExtensionPackagesTests
{
    [Fact]
    public void Execute_Succeeds()
    {
        const string outputPath = "../../../obj/TestProj";
        ResolveExtensionPackages task = new()
        {
            RestoreOutputPath = outputPath,
            BuildEngine = Mock.Of<IBuildEngine>(),
        };

        task.Execute();
    }
}
