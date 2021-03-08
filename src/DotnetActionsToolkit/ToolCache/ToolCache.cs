using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DotnetActionsToolkit
{
    public class ToolCache
    {
        private readonly Core _core = new Core();

        public string CacheDir(string sourceDir, string tool, string version)
        {
            return CacheDir(sourceDir, tool, version, null);
        }

        public string CacheDir(string sourceDir, string tool, string version, string arch)
        {
            if (SemVer.Version.TryParse(version, out var semver))
            {
                version = semver.Clean();
            }

            if (string.IsNullOrWhiteSpace(arch))
            {
                arch = RuntimeInformation.OSArchitecture.ToString();
            }

            _core.Debug($"Caching tool {tool} {version} {arch}");
            _core.Debug($"source dir: {sourceDir}");

            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException("sourceDir is not a directory");
            }

            var destPath = CreateToolPath(tool, version, arch);
            CopyFilesRecursively(new DirectoryInfo(sourceDir), new DirectoryInfo(destPath));
            CompleteToolPath(tool, version, arch);

            return destPath;
        }

        public string CacheFile(string sourceFile, string targetFile, string tool, string version)
        {
            return CacheFile(sourceFile, targetFile, tool, version, null);
        }

        public string CacheFile(string sourceFile, string targetFile, string tool, string version, string arch)
        {
            if (SemVer.Version.TryParse(version, out var semver))
            {
                version = semver.Clean();
            }

            if (string.IsNullOrWhiteSpace(arch))
            {
                arch = RuntimeInformation.OSArchitecture.ToString();
            }

            _core.Debug($"Caching tool {tool} {version} {arch}");
            _core.Debug($"source file: {sourceFile}");

            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException("sourceFile is not a file", sourceFile);
            }

            var destFolder = CreateToolPath(tool, version, arch);
            var destPath = Path.Combine(destFolder, targetFile);
            _core.Debug($"destination file {destPath}");
            File.Copy(sourceFile, destPath);

            CompleteToolPath(tool, version, arch);

            return destFolder;
        }

        public string Find(string toolName, string versionSpec)
        {
            return Find(toolName, versionSpec, null);
        }

        public string Find(string toolName, string versionSpec, string arch)
        {
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
                _core.Debug($"checking cache: {cachePath}");

                if (Directory.Exists(cachePath) && File.Exists($"{cachePath}.complete"))
                {
                    _core.Debug($"Found tool in cache {toolName} {versionSpec} {arch}");
                    return cachePath;
                }

                _core.Debug("not found");
            }

            return null;
        }

        public IEnumerable<string> FindAllVersions(string toolName)
        {
            return FindAllVersions(toolName, null);
        }

        public IEnumerable<string> FindAllVersions(string toolName, string arch)
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

        private string CreateToolPath(string tool, string version, string arch)
        {
            var folderPath = Path.Combine(GetCacheDirectory(), tool, version, arch);

            _core.Debug($"destination {folderPath}");
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

        private void CompleteToolPath(string tool, string version, string arch)
        {
            var folderPath = Path.Combine(GetCacheDirectory(), tool, version, arch);
            var markerPath = $"{folderPath}.complete";
            File.Create(markerPath);
            _core.Debug("finished caching tool");
        }

        private bool IsExplicitVersion(string versionSpec)
        {
            var valid = SemVer.Version.TryParse(versionSpec, out var _);

            _core.Debug($"isExplicit: {versionSpec}");
            _core.Debug($"explicit? {valid}");

            return valid;
        }

        private string EvaluateVersions(IEnumerable<string> versions, string versionSpec)
        {
            _core.Debug($"evaluating {versions.Count()} versions");

            var version = SemVer.Range.MaxSatisfying(versionSpec, versions);

            if (!string.IsNullOrWhiteSpace(version))
            {
                _core.Debug($"matched: {version}");
                
                return version;
            }

            _core.Debug("match not found");
            
            return null;
        }

        private string GetCacheDirectory()
        {
            var cacheDirectory = Environment.GetEnvironmentVariable("RUNNER_TOOL_CACHE");

            if (cacheDirectory == null)
            {
                throw new Exception("Expected RUNNER_TOOL_CACHE to be defined");
            }

            return cacheDirectory;
        }

        private void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
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
