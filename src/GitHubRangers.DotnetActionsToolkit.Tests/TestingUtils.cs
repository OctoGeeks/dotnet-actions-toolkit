using System;
using System.IO;
using System.Reflection;

namespace GitHubRangers.ActionsToolkit.Tests
{
    public static class TestingUtils
    {
        public static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
