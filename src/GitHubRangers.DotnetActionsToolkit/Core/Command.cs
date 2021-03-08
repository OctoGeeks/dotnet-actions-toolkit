using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitHubRangers.DotnetActionsToolkit
{
    internal class Command
    {
        private const string CMD_STRING = "::";

        internal void IssueCommand<T>(string command, Dictionary<string, string> properties, T message)
        {
            Console.WriteLine(BuildCommand(command, properties, message));
        }

        internal void IssueCommand<T>(string command, KeyValuePair<string, string> properties, T message)
        {
            var dict = new Dictionary<string, string>()
            {
                { properties.Key, properties.Value }
            };

            IssueCommand(command, dict, message);
        }

        internal void IssueCommand<T>(string command, T message)
        {
            IssueCommand(command, null, message);
        }

        internal void IssueCommand(string command)
        {
            IssueCommand(command, null, string.Empty);
        }

        internal string BuildCommand<T>(string command, Dictionary<string, string> properties, T message)
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

        internal string EscapeData<T>(T data)
        {
            return data.ToCommandValue().Replace("%", "%25")
                                        .Replace("\r", "%0D")
                                        .Replace("\n", "%0A");
        }

        internal string EscapeProperty<T>(T property)
        {
            return property.ToCommandValue().Replace("%", "%25")
                                            .Replace("\r", "%0D")
                                            .Replace("\n", "%0A")
                                            .Replace(":", "%3A")
                                            .Replace(",", "%2C");
        }
    }
}
