using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace DotnetActionsToolkit.Tests
{
    [TestClass]
    public class ToolCacheTests
    {
        private static readonly string CACHE_PATH = Path.Combine(TestingUtils.GetAssemblyDirectory(), "CACHE");
        private static readonly string TEMP_PATH = Path.Combine(TestingUtils.GetAssemblyDirectory(), "TEMP");
        private readonly ToolCache _toolCache = new ToolCache();

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
            var retries = 0;

            while (retries < 10)
            {
                try
                {
                    if (Directory.Exists(CACHE_PATH))
                    {
                        Directory.Delete(CACHE_PATH, true);
                    }

                    if (Directory.Exists(TEMP_PATH))
                    {
                        Directory.Delete(TEMP_PATH, true);
                    }
                }
                catch (IOException)
                {
                    retries++;
                }
            }
        }

        [TestMethod]
        public void InstallsABinaryToolAndFindsIt()
        {
            var downPath = FakeDownloadTool();

            Assert.IsTrue(File.Exists(downPath));

            _toolCache.CacheFile(downPath, "foo", "foo", "1.1.0");

            var toolPath = _toolCache.Find("foo", "1.1.0");

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
