using System;
using System.IO;
using System.Text;

namespace DotnetActionsToolkit
{
    internal class FileCommand
    {
        internal void IssueFileCommand<T>(string command, T message)
        {
            var filePath = Environment.GetEnvironmentVariable($"GITHUB_{command}");

            if (filePath == null)
            {
                throw new ArgumentException($"Unable to find environment variable for file command {command}", nameof(command));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Missing file at path: {filePath}", filePath);
            }

            File.AppendAllText(filePath, $"{message.ToCommandValue()}{Environment.NewLine}", Encoding.UTF8);
        }
    }
}
