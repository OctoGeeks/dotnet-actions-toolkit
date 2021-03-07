using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace GitHubRangers.ActionsToolkit
{
    public static class Core
    {
        private const string CMD_STRING = "::";

        public static void ExportVariable<T>(string name, T val)
        {
            var convertedVal = ToCommandValue(val);
            Environment.SetEnvironmentVariable(name, convertedVal);

            var filePath = Environment.GetEnvironmentVariable("GITHUB_ENV");

            if (filePath != null)
            {
                var delimiter = "_GitHubActionsFileCommandDelimeter_";
                var commandValue = $"{name}<<{delimiter}{Environment.NewLine}{convertedVal}{Environment.NewLine}{delimiter}";
                IssueFileCommand("ENV", commandValue);
            } else
            {
                IssueCommand("set-env", new KeyValuePair<string, string>("name", name), convertedVal);
            }
        }

        public static void SetSecret(string secret)
        {
            IssueCommand("add-mask", secret);
        }

        public static void AddPath(string inputPath)
        {
            var filePath = Environment.GetEnvironmentVariable("GITHUB_PATH");

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                IssueFileCommand("PATH", inputPath);
            }
            else
            {
                IssueCommand("add-path", inputPath);
            }

            Environment.SetEnvironmentVariable("PATH", $"{inputPath}{Path.PathSeparator}{Environment.GetEnvironmentVariable("PATH")}");
        }

        public static string GetInput(string name, bool required)
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

        public static string GetInput(string name)
        {
            return GetInput(name, false);
        }

        public static void SetOutput<T>(string name, T value)
        {
            IssueCommand("set-output", new KeyValuePair<string, string>("name", name), value);
        }

        public static void SetCommandEcho(bool enabled)
        {
            IssueCommand("echo", enabled ? "on" : "off");
        }

        public static void SetFailed(string message)
        {
            Environment.ExitCode = 1;

            Error(message);
        }

        public static void SetFailed(Exception ex)
        {
            Environment.ExitCode = 1;

            Error(ex);
        }

        public static bool IsDebug()
        {
            return Environment.GetEnvironmentVariable("RUNNER_DEBUG") == "1";
        }

        public static void Debug(string message)
        {
            IssueCommand("debug", message);
        }

        public static void Error(string message)
        {
            IssueCommand("error", message);
        }

        public static void Error(Exception ex)
        {
            Error(ex.ToString());
        }

        public static void Warning(string message)
        {
            IssueCommand("warning", message);
        }

        public static void Warning(Exception ex)
        {
            Warning(ex.ToString());
        }

        public static void Info(string message)
        {
            Console.WriteLine(message);
        }

        public static void StartGroup(string name)
        {
            IssueCommand("group", name);
        }

        public static void EndGroup()
        {
            IssueCommand("endgroup");
        }

        public static T Group<T>(string name, Func<T> fn)
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

        public static void SaveState<T>(string name, T value)
        {
            IssueCommand("save-state", new KeyValuePair<string, string>("name", name), value);
        }

        public static string GetState(string name)
        {
            return Environment.GetEnvironmentVariable($"STATE_{name}") ?? string.Empty;
        }

        private static void IssueCommand<T>(string command, Dictionary<string, string> properties, T message)
        {
            Console.WriteLine(BuildCommand(command, properties, message));
        }

        private static void IssueCommand<T>(string command, KeyValuePair<string, string> properties, T message)
        {
            var dict = new Dictionary<string, string>()
            {
                { properties.Key, properties.Value }
            };

            IssueCommand(command, dict, message);
        }

        private static void IssueCommand<T>(string command, T message)
        {
            IssueCommand(command, null, message);
        }

        private static void IssueCommand(string command)
        {
            IssueCommand(command, null, string.Empty);
        }

        private static string BuildCommand<T>(string command, Dictionary<string, string> properties, T message)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                command = "missing.command";
            }

            var sb = new StringBuilder();

            sb.Append(CMD_STRING);
            sb.Append(command);

            if (properties != null && properties.Any())
            {
                sb.Append(" ");

                var props = properties.Where(p => !string.IsNullOrWhiteSpace(p.Value) && !string.IsNullOrWhiteSpace(p.Key))
                                      .Select(p => $"{p.Key}={EscapeProperty(p.Value)}");

                sb.Append(string.Join(",", props));
            }

            sb.Append(CMD_STRING);
            sb.Append(EscapeData(message));

            return sb.ToString();
        }

        private static string EscapeProperty<T>(T property)
        {
            return ToCommandValue(property).Replace("%", "%25")
                                           .Replace("\r", "%0D")
                                           .Replace("\n", "%0A")
                                           .Replace(":", "%3A")
                                           .Replace(",", "%2C");
        }

        private static string EscapeData<T>(T data)
        {
            return ToCommandValue(data).Replace("%", "%25")
                                       .Replace("\r", "%0D")
                                       .Replace("\n", "%0A");
        }

        private static void IssueFileCommand<T>(string command, T message)
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

            File.AppendAllText(filePath, $"{ToCommandValue(message)}{Environment.NewLine}", Encoding.UTF8);
        }

        private static string ToCommandValue<T>(T input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            // TODO: probably better to jsonify it if its anything other than a string
            return input.ToString();
        }
    }
}
