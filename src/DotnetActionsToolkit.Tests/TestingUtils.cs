using System;
using System.IO;
using System.Reflection;

namespace DotnetActionsToolkit.Tests
{
    public static class TestingUtils
    {
        public static string GetAssemblyDirectory()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(location);
        }
    }
}
