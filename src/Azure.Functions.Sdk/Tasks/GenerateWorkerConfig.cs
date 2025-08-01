// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Text.Json;
using Microsoft.Build.Framework;

namespace Azure.Functions.Sdk.Tasks;

public class GenerateWorkerConfig(IFileSystem fileSystem) : Microsoft.Build.Utilities.Task
{
    private readonly IFileSystem _fileSystem = Throw.IfNull(fileSystem);

    public GenerateWorkerConfig()
        : this(new FileSystem())
    {
    }

    [Required]
    public string Executable { get; set; } = string.Empty;

    [Required]
    public string TargetFileName { get; set; } = string.Empty;

    [Required]
    public string OutputPath { get; set; } = string.Empty;

    public override bool Execute()
    {
        Config config = new(Executable, TargetFileName);
        string json = JsonSerializer.Serialize(config, GeneratedJsonContext.Default.Config);
        _fileSystem.File.WriteAllText(OutputPath, json);
        return true;
    }

    public class Config(string executable, string path)
    {
        public Description Description { get; } = new(executable, path);
    }

    public class Description(string executable, string path)
    {
        public string Language => "dotnet-isolated";

        public IEnumerable<string> Extensions => [".dll"];

        public string DefaultExecutablePath => executable;

        public string DefaultWorkerPath => path;

        public string WorkerIndexing => "true";

        public bool CanUsePlaceholder => true;
    }
}
