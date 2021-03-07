using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GitHubRangers.ActionsToolkit.Tests
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void CommandOnly()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                IssueCommand("some-command");
                Assert.AreEqual($"::some-command::{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void CommandEscapesMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                IssueCommand("some-command", "percent % percent % cr \r cr \r lf \n lf \n");
                Assert.AreEqual($"::some-command::percent %25 percent %25 cr %0D cr %0D lf %0A lf %0A{Environment.NewLine}", consoleOutput.GetOuput());
            }

            using (var consoleOutput = new ConsoleOutput())
            {
                IssueCommand("some-command", "%25 %25 %0D %0D %0A %0A");
                Assert.AreEqual($"::some-command::%2525 %2525 %250D %250D %250A %250A{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void CommandEscapesProperty()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                IssueCommand("some-command", new KeyValuePair<string, string>("name", "percent % percent % cr \r cr \r lf \n lf \n colon : colon : comma , comma ,"), "");
                Assert.AreEqual($"::some-command name=percent %25 percent %25 cr %0D cr %0D lf %0A lf %0A colon %3A colon %3A comma %2C comma %2C::{Environment.NewLine}", consoleOutput.GetOuput());
            }

            using (var consoleOutput = new ConsoleOutput())
            {
                IssueCommand("some-command", "%25 %25 %0D %0D %0A %0A %3A %3A %2C %2C");
                Assert.AreEqual($"::some-command::%2525 %2525 %250D %250D %250A %250A %253A %253A %252C %252C{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void CommandWithMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                IssueCommand("some-command", "some message");
                Assert.AreEqual($"::some-command::some message{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void CommandWithMessageAndProperties()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                var props = new Dictionary<string, string>();
                props.Add("prop1", "value 1");
                props.Add("prop2", "value 2");

                IssueCommand("some-command", props, "some message");
                Assert.AreEqual($"::some-command prop1=value 1,prop2=value 2::some message{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void CommandWithOneProperty()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                IssueCommand("some-command", new KeyValuePair<string, string>("prop1", "value 1"), "");
                Assert.AreEqual($"::some-command prop1=value 1::{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void CommandWithTwoProperties()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                var props = new Dictionary<string, string>();
                props.Add("prop1", "value 1");
                props.Add("prop2", "value 2");

                IssueCommand("some-command", props, "");
                Assert.AreEqual($"::some-command prop1=value 1,prop2=value 2::{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void CommandWithThreeProperties()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                var props = new Dictionary<string, string>();
                props.Add("prop1", "value 1");
                props.Add("prop2", "value 2");
                props.Add("prop3", "value 3");

                IssueCommand("some-command", props, "");
                Assert.AreEqual($"::some-command prop1=value 1,prop2=value 2,prop3=value 3::{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        private void IssueCommand<T>(string command, Dictionary<string, string> properties, T message)
        {
            var method = typeof(Core).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                     .Single(m => m.Name == "IssueCommand" && 
                                                  m.IsGenericMethod && 
                                                  m.GetParameters().Length == 3 && 
                                                  m.GetParameters()[1].ParameterType == typeof(Dictionary<string, string>));

            var genericMethod = method.MakeGenericMethod(typeof(T));

            genericMethod.Invoke(null, new object[] { command, properties, message });
        }

        private void IssueCommand<T>(string command, KeyValuePair<string, string> properties, T message)
        {
            var method = typeof(Core).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                     .Single(m => m.Name == "IssueCommand" &&
                                                  m.IsGenericMethod &&
                                                  m.GetParameters().Length == 3 &&
                                                  m.GetParameters()[1].ParameterType == typeof(KeyValuePair<string, string>));

            var genericMethod = method.MakeGenericMethod(typeof(T));

            genericMethod.Invoke(null, new object[] { command, properties, message });
        }

        private void IssueCommand<T>(string command, T message)
        {
            var method = typeof(Core).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                     .Single(m => m.Name == "IssueCommand" &&
                                                  m.IsGenericMethod &&
                                                  m.GetParameters().Length == 2);

            var genericMethod = method.MakeGenericMethod(typeof(T));

            genericMethod.Invoke(null, new object[] { command, message });
        }

        private void IssueCommand(string command)
        {
            var method = typeof(Core).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                     .Single(m => m.Name == "IssueCommand" &&
                                                  m.GetParameters().Length == 1);

            method.Invoke(null, new object[] { command });
        }
    }
}
