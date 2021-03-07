using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;

namespace GitHubRangers.ActionsToolkit.Tests
{
    [TestClass]
    public class ToolCacheTests
    {
        private static readonly string CACHE_PATH = Path.Combine(TestingUtils.GetAssemblyDirectory(), "CACHE");
        private static readonly string TEMP_PATH = Path.Combine(TestingUtils.GetAssemblyDirectory(), "TEMP");

        [TestInitialize]
        public void TestInitialize()
        {
            Environment.SetEnvironmentVariable("RUNNER_TOOL_CACHE", CACHE_PATH);

            if (Directory.Exists(CACHE_PATH))
            {
                Directory.Delete(CACHE_PATH, true);
            }

            if (Directory.Exists(TEMP_PATH))
            {
                Directory.Delete(TEMP_PATH, true);
            }

            Directory.CreateDirectory(CACHE_PATH);
            Directory.CreateDirectory(TEMP_PATH);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Directory.Delete(CACHE_PATH, true);
            Directory.Delete(TEMP_PATH, true);
        }

        [TestMethod]
        public void InstallsABinaryToolAndFindsIt()
        {
            var downPath = FakeDownloadTool();

            Assert.IsTrue(File.Exists(downPath));

            ToolCache.CacheFile(downPath, "foo", "foo", "1.1.0");

            var toolPath = ToolCache.Find("foo", "1.1.0");

            Assert.IsTrue(Directory.Exists(toolPath));
            Assert.IsTrue(File.Exists($"{toolPath}.complete"));

            var binaryPath = Path.Join(toolPath, "foo");
            Assert.IsTrue(File.Exists(binaryPath));
        }

        private string FakeDownloadTool()
        {
            var fileName = Guid.NewGuid().ToString();
            var filePath = Path.Combine(TEMP_PATH, fileName);

            File.Delete(filePath);
            File.WriteAllText(filePath, "sample data");

            return filePath;
        }
    }
}
