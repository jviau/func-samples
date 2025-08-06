// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Build.Framework;

// IMPORTANT: Do not modify this file directly with major changes
// This file is a copy from this project (with minor updates) -- https://github.com/Azure/azure-functions-vs-build-sdk/blob/b0e54a832a92119e00a2b1796258fcf88e0d6109/src/Microsoft.NET.Sdk.Functions.MSBuild/Microsoft.NET.Sdk.Functions.MSBuild.csproj
// Please make any changes upstream first.

namespace Azure.Functions.Sdk.Tasks.Publish;

public class CreateZipFile : Microsoft.Build.Utilities.Task
{
    internal const string WorkerRootReplacement = "{WorkerRoot}";

    // Unix file permissions for -rwxrwxrwx
    internal static readonly int UnixExecutablePermissions = Convert.ToInt32("100777", 8) << 16;

    [Required]
    public string FolderToZip { get; set; } = string.Empty;

    [Required]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    public string PublishIntermediateTempPath { get; set; } = string.Empty;

    [Required]
    public string Executable { get; set; } = string.Empty;

    [Output]
    public string CreatedZipPath { get; private set; } = string.Empty;

    public override bool Execute()
    {
        if (!Path.IsPathRooted(FolderToZip))
        {
            Log.LogError($"The path '{FolderToZip}' is not rooted. Please provide an absolute path for FolderToZip.");
            return false;
        }

        if (!Path.IsPathRooted(PublishIntermediateTempPath))
        {
            Log.LogError($"The path '{PublishIntermediateTempPath}' is not rooted. Please provide an absolute path for PublishIntermediateTempPath.");
            return false;
        }

        CreatedZipPath = CreateZipFileFromDirectory();
        return true;
    }

    internal static void ModifyUnixFilePermissions(string zipFilePath, string entryRootPath, string entryName)
    {
        using ZipFile zipFile = new(zipFilePath);
        zipFile.EntryFactory = new EntryFactory();
        zipFile.BeginUpdate();

        try
        {
            string entryFullPath = Path.Combine(entryRootPath, entryName);

            // In Windows, we leave off the .exe, so need to adjust the entry name
            if (!File.Exists(entryFullPath))
            {
                entryFullPath += ".exe";
                entryName += ".exe";

                if (!File.Exists(entryFullPath))
                {
                    return;
                }
            }

            // This will overwrite the existing entry.
            zipFile.Add(entryFullPath, entryName);
        }
        finally
        {
            zipFile.CommitUpdate();
            zipFile.Close();
        }
    }

    internal string CreateZipFileFromDirectory()
    {
        string zipFileName = ProjectName + " - " + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".zip";
        string destination = Path.Combine(PublishIntermediateTempPath, zipFileName);
        System.IO.Compression.ZipFile.CreateFromDirectory(FolderToZip, destination);

        if (Executable.StartsWith(WorkerRootReplacement, StringComparison.OrdinalIgnoreCase))
        {
            string executable = Executable.Replace(WorkerRootReplacement, string.Empty);
            if (!string.IsNullOrEmpty(executable))
            {
                ModifyUnixFilePermissions(destination, FolderToZip, executable);
            }
        }

        return destination;
    }

    private class EntryFactory : IEntryFactory
    {
        private readonly IEntryFactory _internal = new ZipEntryFactory();

        public INameTransform NameTransform { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ZipEntryFactory.TimeSetting Setting => throw new NotImplementedException();

        public DateTime FixedDateTime => throw new NotImplementedException();

        public ZipEntry MakeDirectoryEntry(string directoryName)
        {
            throw new NotImplementedException();
        }

        public ZipEntry MakeDirectoryEntry(string directoryName, bool useFileSystem)
        {
            throw new NotImplementedException();
        }

        public ZipEntry MakeFileEntry(string fileName)
        {
            throw new NotImplementedException();
        }

        public ZipEntry MakeFileEntry(string fileName, bool useFileSystem)
        {
            throw new NotImplementedException();
        }

        public ZipEntry MakeFileEntry(string fileName, string entryName, bool useFileSystem)
        {
            ZipEntry zipEntry = _internal.MakeFileEntry(fileName, entryName, useFileSystem);
            zipEntry.HostSystem = 3; // Unix
            zipEntry.ExternalFileAttributes = UnixExecutablePermissions; // Unix file permissions for -rwxrwxrwx
            return zipEntry;
        }
    }
}
