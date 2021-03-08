using System;
using System.Collections.Generic;
using System.IO;

namespace DotnetActionsToolkit
{
    public class Core
    {
        private readonly Command _command = new Command();
        private readonly FileCommand _fileCommand = new FileCommand();

        public void ExportVariable<T>(string name, T val)
        {
            var convertedVal = val.ToCommandValue();
            Environment.SetEnvironmentVariable(name, convertedVal);

            var filePath = Environment.GetEnvironmentVariable("GITHUB_ENV");

            if (filePath != null)
            {
                var delimiter = "_GitHubActionsFileCommandDelimeter_";
                var commandValue = $"{name}<<{delimiter}{Environment.NewLine}{convertedVal}{Environment.NewLine}{delimiter}";
                _fileCommand.IssueFileCommand("ENV", commandValue);
            } else
            {
                _command.IssueCommand("set-env", new KeyValuePair<string, string>("name", name), convertedVal);
            }
        }

        public void SetSecret(string secret)
        {
            _command.IssueCommand("add-mask", secret);
        }

        public void AddPath(string inputPath)
        {
            var filePath = Environment.GetEnvironmentVariable("GITHUB_PATH");

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _fileCommand.IssueFileCommand("PATH", inputPath);
            }
            else
            {
                _command.IssueCommand("add-path", inputPath);
            }

            Environment.SetEnvironmentVariable("PATH", $"{inputPath}{Path.PathSeparator}{Environment.GetEnvironmentVariable("PATH")}");
        }

        public string GetInput(string name)
        {
            return GetInput(name, false);
        }

        public string GetInput(string name, bool required)
        {
            var envVariableName = $"INPUT_{name.Replace(' ', '_').ToUpperInvariant()}";
            var val = Environment.GetEnvironmentVariable(envVariableName);

            if (required && val == null)
            {
                throw new Exception($"Input required and not supplied: {name}");
            }

            if (val != null)
            {
                return val.Trim();
            }

            return val;
        }

        public void SetOutput<T>(string name, T value)
        {
            _command.IssueCommand("set-output", new KeyValuePair<string, string>("name", name), value);
        }

        public void SetCommandEcho(bool enabled)
        {
            _command.IssueCommand("echo", enabled ? "on" : "off");
        }

        public void SetFailed(string message)
        {
            Environment.ExitCode = 1;
            Error(message);
        }

        public void SetFailed(Exception ex)
        {
            Environment.ExitCode = 1;
            Error(ex);
        }

        public bool IsDebug()
        {
            return Environment.GetEnvironmentVariable("RUNNER_DEBUG") == "1";
        }

        public void Debug(string message)
        {
            _command.IssueCommand("debug", message);
        }

        public void Error(string message)
        {
            _command.IssueCommand("error", message);
        }

        public void Error(Exception ex)
        {
            Error(ex.ToString());
        }

        public void Warning(string message)
        {
            _command.IssueCommand("warning", message);
        }

        public void Warning(Exception ex)
        {
            Warning(ex.ToString());
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void StartGroup(string name)
        {
            _command.IssueCommand("group", name);
        }

        public void EndGroup()
        {
            _command.IssueCommand("endgroup");
        }

        public T Group<T>(string name, Func<T> fn)
        {
            StartGroup(name);

            try
            {
                return fn();
            }
            finally
            {
                EndGroup();
            }
        }

        public void SaveState<T>(string name, T value)
        {
            _command.IssueCommand("save-state", new KeyValuePair<string, string>("name", name), value);
        }

        public string GetState(string name)
        {
            return Environment.GetEnvironmentVariable($"STATE_{name}") ?? string.Empty;
        }
    }
}
