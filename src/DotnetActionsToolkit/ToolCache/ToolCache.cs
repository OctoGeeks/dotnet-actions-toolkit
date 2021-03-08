using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DotnetActionsToolkit
{
    public static class ToolCache
    {
        public static string CacheDir(string sourceDir, string tool, string version)
        {
            return CacheDir(sourceDir, tool, version, null);
        }

        public static string CacheDir(string sourceDir, string tool, string version, string arch)
        {
            var core = new Core();

            if (SemVer.Version.TryParse(version, out var semver))
            {
                version = semver.Clean();
            }

            if (string.IsNullOrWhiteSpace(arch))
            {
                arch = RuntimeInformation.OSArchitecture.ToString();
            }

            core.Debug($"Caching tool {tool} {version} {arch}");
            core.Debug($"source dir: {sourceDir}");

            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException("sourceDir is not a directory");
            }

            var destPath = CreateToolPath(tool, version, arch);
            CopyFilesRecursively(new DirectoryInfo(sourceDir), new DirectoryInfo(destPath));
            CompleteToolPath(tool, version, arch);

            return destPath;
        }

        public static string CacheFile(string sourceFile, string targetFile, string tool, string version)
        {
            return CacheFile(sourceFile, targetFile, tool, version, null);
        }

        public static string CacheFile(string sourceFile, string targetFile, string tool, string version, string arch)
        {
            var core = new Core();

            if (SemVer.Version.TryParse(version, out var semver))
            {
                version = semver.Clean();
            }

            if (string.IsNullOrWhiteSpace(arch))
            {
                arch = RuntimeInformation.OSArchitecture.ToString();
            }

            core.Debug($"Caching tool {tool} {version} {arch}");
            core.Debug($"source file: {sourceFile}");

            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException("sourceFile is not a file", sourceFile);
            }

            var destFolder = CreateToolPath(tool, version, arch);
            var destPath = Path.Combine(destFolder, targetFile);
            core.Debug($"destination file {destPath}");
            File.Copy(sourceFile, destPath);

            CompleteToolPath(tool, version, arch);

            return destFolder;
        }

        public static string Find(string toolName, string versionSpec)
        {
            return Find(toolName, versionSpec, null);
        }

        public static string Find(string toolName, string versionSpec, string arch)
        {
            var core = new Core();

            if (string.IsNullOrWhiteSpace(toolName))
            {
                throw new ArgumentException("toolName parameter is required", nameof(toolName));
            }

            if (string.IsNullOrWhiteSpace(versionSpec))
            {
                throw new ArgumentException("versionSpec parameter is required", nameof(versionSpec));
            }

            if (string.IsNullOrWhiteSpace(arch))
            {
                arch = RuntimeInformation.OSArchitecture.ToString();
            }

            if (!IsExplicitVersion(versionSpec))
            {
                var localVersions = FindAllVersions(toolName, arch);
                var match = EvaluateVersions(localVersions, versionSpec);
                versionSpec = match;
            }

            if (!string.IsNullOrWhiteSpace(versionSpec))
            {
                if (SemVer.Version.TryParse(versionSpec, out var semver))
                {
                    versionSpec = semver.Clean();
                }

                var cachePath = Path.Combine(GetCacheDirectory(), toolName, versionSpec, arch);
                core.Debug($"checking cache: {cachePath}");

                if (Directory.Exists(cachePath) && File.Exists($"{cachePath}.complete"))
                {
                    core.Debug($"Found tool in cache {toolName} {versionSpec} {arch}");
                    return cachePath;
                }

                core.Debug("not found");
            }

            return null;
        }

        public static IEnumerable<string> FindAllVersions(string toolName)
        {
            return FindAllVersions(toolName, null);
        }

        public static IEnumerable<string> FindAllVersions(string toolName, string arch)
        {
            var versions = new List<string>();

            if (string.IsNullOrWhiteSpace(arch))
            {
                arch = RuntimeInformation.OSArchitecture.ToString();
            }

            var toolPath = Path.Combine(GetCacheDirectory(), toolName);

            if (Directory.Exists(toolPath))
            {
                foreach (var dir in Directory.GetDirectories(toolPath))
                {
                    if (IsExplicitVersion(Path.GetDirectoryName(dir)))
                    {
                        var fullPath = Path.Combine(dir, arch);

                        if (Directory.Exists(fullPath) && File.Exists($"{fullPath}.complete"))
                        {
                            versions.Add(Path.GetDirectoryName(dir));
                        }
                    }
                }
            }

            return versions;
        }

        private static string CreateToolPath(string tool, string version, string arch)
        {
            var core = new Core();
            var folderPath = Path.Combine(GetCacheDirectory(), tool, version, arch);

            core.Debug($"destination {folderPath}");
            var markerPath = $"{folderPath}.complete";
            
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }

            if (File.Exists(markerPath))
            {
                File.Delete(markerPath);
            }

            Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        private static void CompleteToolPath(string tool, string version, string arch)
        {
            var folderPath = Path.Combine(GetCacheDirectory(), tool, version, arch);
            var markerPath = $"{folderPath}.complete";
            File.Create(markerPath);
            new Core().Debug("finished caching tool");
        }

        private static bool IsExplicitVersion(string versionSpec)
        {
            var core = new Core();
            var valid = SemVer.Version.TryParse(versionSpec, out var _);

            core.Debug($"isExplicit: {versionSpec}");
            core.Debug($"explicit? {valid}");

            return valid;
        }

        private static string EvaluateVersions(IEnumerable<string> versions, string versionSpec)
        {
            var core = new Core();

            core.Debug($"evaluating {versions.Count()} versions");

            var version = SemVer.Range.MaxSatisfying(versionSpec, versions);

            if (!string.IsNullOrWhiteSpace(version))
            {
                core.Debug($"matched: {version}");
                
                return version;
            }

            core.Debug("match not found");
            
            return null;
        }

        private static string GetCacheDirectory()
        {
            var cacheDirectory = Environment.GetEnvironmentVariable("RUNNER_TOOL_CACHE");

            if (cacheDirectory == null)
            {
                throw new Exception("Expected RUNNER_TOOL_CACHE to be defined");
            }

            return cacheDirectory;
        }

        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }
    }
}
