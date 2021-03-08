using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace GitHubRangers.DotnetActionsToolkit.Tests
{
    [TestClass]
    public class CoreTests
    {
        private const string DELIM = "_GitHubActionsFileCommandDelimeter_";

        [ClassInitialize]
        public static void ClassInitialize(TestContext ctx)
        {
            var filePath = Path.Combine(TestingUtils.GetAssemblyDirectory(), "test");

            Directory.CreateDirectory(filePath);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Environment.SetEnvironmentVariable("my var", "");
            Environment.SetEnvironmentVariable("special char var \r\n];", "");
            Environment.SetEnvironmentVariable("my var2", "");
            Environment.SetEnvironmentVariable("my secret", "");
            Environment.SetEnvironmentVariable("special char secret \r\n];", "");
            Environment.SetEnvironmentVariable("my secret2", "");
            Environment.SetEnvironmentVariable("PATH", $"path1{Path.PathSeparator}path2");
            Environment.SetEnvironmentVariable("INPUT_MY_INPUT", "val");
            Environment.SetEnvironmentVariable("INPUT_MISSING", "");
            Environment.SetEnvironmentVariable("INPUT_SPECIAL_CHARS_\'\t\"\\", "\'\t\"\\ response");
            Environment.SetEnvironmentVariable("INPUT_MULTIPLE_SPACES_VARIABLE", "I have multiple spaces");
            Environment.SetEnvironmentVariable("STATE_TEST_1", "state_val");
            Environment.SetEnvironmentVariable("GITHUB_PATH", "");
            Environment.SetEnvironmentVariable("GITHUB_ENV", "");
        }

        [TestMethod]
        public void LegacyExportVariableProducesTheCorrectCommandAndSetsTheEnv()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().ExportVariable("my var", "var val");

                Assert.AreEqual($"::set-env name=my var::var val{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void LegacyExportVariableEscapesVariableNames()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().ExportVariable("special char var \r\n,:", "special val");

                Assert.AreEqual("special val", Environment.GetEnvironmentVariable("special char var \r\n,:"));
                Assert.AreEqual($"::set-env name=special char var %0D%0A%2C%3A::special val{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void LegacyExportVariableEscapesVariableValues()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().ExportVariable("my var2", "var val\r\n");

                Assert.AreEqual("var val\r\n", Environment.GetEnvironmentVariable("my var2"));
                Assert.AreEqual($"::set-env name=my var2::var val%0D%0A{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void LegacyExportVariableHandlesBooleanInputs()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().ExportVariable("my var", true);

                Assert.AreEqual($"::set-env name=my var::True{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void LegacyExportVariableHandlesNumberInputs()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().ExportVariable("my var", 5);

                Assert.AreEqual($"::set-env name=my var::5{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void ExportVariableProducesTheCorrectCommandAndSetsTheEnv()
        {
            var command = "ENV";
            CreateFileCommandFile(command);

            new Core().ExportVariable("my var", "var val");
            VerifyFileCommand(command, $"my var<<{DELIM}{Environment.NewLine}var val{Environment.NewLine}{DELIM}{Environment.NewLine}");
        }

        [TestMethod]
        public void ExportVariableHandlesBooleanInputs()
        {
            var command = "ENV";
            CreateFileCommandFile(command);

            new Core().ExportVariable("my var", true);
            VerifyFileCommand(command, $"my var<<{DELIM}{Environment.NewLine}True{Environment.NewLine}{DELIM}{Environment.NewLine}");
        }

        [TestMethod]
        public void ExportVariableHandlesNumberInputs()
        {
            var command = "ENV";
            CreateFileCommandFile(command);

            new Core().ExportVariable("my var", 5);
            VerifyFileCommand(command, $"my var<<{DELIM}{Environment.NewLine}5{Environment.NewLine}{DELIM}{Environment.NewLine}");
        }

        [TestMethod]
        public void SetSecretProducesTheCorrectCommand()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SetSecret("secret val");

                Assert.AreEqual($"::add-mask::secret val{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void PrependPathProducesTheCorrectCommandsAndSetsTheEnv()
        {
            var command = "PATH";
            CreateFileCommandFile(command);
            new Core().AddPath("myPath");

            Assert.AreEqual($"myPath{Path.PathSeparator}path1{Path.PathSeparator}path2", Environment.GetEnvironmentVariable("PATH"));
            VerifyFileCommand(command, $"myPath{Environment.NewLine}");
        }

        [TestMethod]
        public void LegacyPrependPathProducesTheCorrectCommandsAndSetsTheEnv()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().AddPath("myPath");

                Assert.AreEqual($"myPath{Path.PathSeparator}path1{Path.PathSeparator}path2", Environment.GetEnvironmentVariable("PATH"));
                Assert.AreEqual($"::add-path::myPath{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void GetInputGetsNonRequiredInput()
        {
            Assert.AreEqual("val", new Core().GetInput("my input"));
        }

        [TestMethod]
        public void GetInputGetsRequiredInput()
        {
            Assert.AreEqual("val", new Core().GetInput("my input", true));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetInputThrowsOnMissingRequiredInput()
        {
            new Core().GetInput("missing", true);
        }

        [TestMethod]
        public void GetInputDoesNotThrowOnMissingNonRequiredInput()
        {
            Assert.AreEqual(null, new Core().GetInput("missing", false));
        }

        [TestMethod]
        public void GetInputIsCaseInsensitive()
        {
            Assert.AreEqual("val", new Core().GetInput("My InPuT"));
        }

        [TestMethod]
        public void GetInputHandlesSpecialCharacters()
        {
            Assert.AreEqual("\'\t\"\\ response", new Core().GetInput("special chars_\'\t\"\\"));
        }

        [TestMethod]
        public void GetInputHandlesMultipleSpaces()
        {
            Assert.AreEqual("I have multiple spaces", new Core().GetInput("multiple spaces variable"));
        }

        [TestMethod]
        public void SetOutputProducesTheCorrectCommand()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SetOutput("some output", "some value");
                Assert.AreEqual($"::set-output name=some output::some value{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SetOutputHandlesBools()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SetOutput("some output", false);
                Assert.AreEqual($"::set-output name=some output::False{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SetOutputHandlesNumbers()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SetOutput("some output", 1.01);
                Assert.AreEqual($"::set-output name=some output::1.01{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SetFailedSetsTheCorrectExitCodeAndFailureMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SetFailed("Failure message");
                Assert.AreEqual(1, Environment.ExitCode);
                Assert.AreEqual($"::error::Failure message{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SetFailedEscapesTheFailureMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SetFailed("Failure \r\n\nmessage\r");
                Assert.AreEqual(1, Environment.ExitCode);
                Assert.AreEqual($"::error::Failure %0D%0A%0Amessage%0D{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SetFailedHandlesError()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                var message = "this is my error message";
                new Core().SetFailed(new Exception(message));
                Assert.AreEqual(1, Environment.ExitCode);
                Assert.AreEqual($"::error::System.Exception: {message}{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void ErrorSetsTheCorrectErrorMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().Error("Error message");
                Assert.AreEqual($"::error::Error message{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void ErrorEscapesTheErrorMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().Error("Error message\r\n\n");
                Assert.AreEqual($"::error::Error message%0D%0A%0A{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void ErrorHandlesAnErrorObject()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                var message = "this is my error message";
                new Core().Error(new Exception(message));
                Assert.AreEqual($"::error::System.Exception: {message}{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void WarningSetsTheCorrectErrorMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().Warning("Warning");
                Assert.AreEqual($"::warning::Warning{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void WarningEscapesTheErrorMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().Warning("\r\nwarning\n");
                Assert.AreEqual($"::warning::%0D%0Awarning%0A{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void WarningHandlesAnErrorObject()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                var message = "this is my error message";
                new Core().Warning(new Exception(message));
                Assert.AreEqual($"::warning::System.Exception: {message}{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void StartGroupStartsANewGroup()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().StartGroup("my-group");
                Assert.AreEqual($"::group::my-group{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void EndGroupEndsNewGroup()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().EndGroup();
                Assert.AreEqual($"::endgroup::{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void GroupWrapsAnAsyncCallInAGroup()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                var result = new Core().Group("mygroup", () =>
                {
                    Console.WriteLine("in my group");
                    return true;
                });

                Assert.IsTrue(result);
                Assert.AreEqual($"::group::mygroup{Environment.NewLine}in my group{Environment.NewLine}::endgroup::{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void DebugSetsTheCorrectMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().Debug("Debug");
                Assert.AreEqual($"::debug::Debug{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void DebugEscapesTheMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().Debug("\r\ndebug\n");
                Assert.AreEqual($"::debug::%0D%0Adebug%0A{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SaveStateProducesTheCorrectCommand()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SaveState("state_1", "some value");
                Assert.AreEqual($"::save-state name=state_1::some value{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SaveStateHandlesNumbers()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SaveState("state_1", 1);
                Assert.AreEqual($"::save-state name=state_1::1{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SaveStateHandlesBools()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SaveState("state_1", true);
                Assert.AreEqual($"::save-state name=state_1::True{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void GetStateGetsWrapperActionState()
        {
            Assert.AreEqual("state_val", new Core().GetState("TEST_1"));
        }

        [TestMethod]
        public void IsDebugCheckDebugState()
        {
            var current = Environment.GetEnvironmentVariable("RUNNER_DEBUG");

            try
            {
                Environment.SetEnvironmentVariable("RUNNER_DEBUG", null);
                Assert.IsFalse(new Core().IsDebug());

                Environment.SetEnvironmentVariable("RUNNER_DEBUG", "1");
                Assert.IsTrue(new Core().IsDebug());
            }
            finally
            {
                Environment.SetEnvironmentVariable("RUNNER_DEBUG", current);
            }
        }

        [TestMethod]
        public void SetCommandEchoCanEnableEchoing()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SetCommandEcho(true);
                Assert.AreEqual($"::echo::on{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        [TestMethod]
        public void SetCommandEchoCanDisableEchoing()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                new Core().SetCommandEcho(false);
                Assert.AreEqual($"::echo::off{Environment.NewLine}", consoleOutput.GetOuput());
            }
        }

        private void CreateFileCommandFile(string command)
        {
            var filePath = Path.Combine(TestingUtils.GetAssemblyDirectory(), "test", command);
            Environment.SetEnvironmentVariable($"GITHUB_{command}", filePath);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.AppendAllText(filePath, string.Empty, Encoding.UTF8);
        }

        private void VerifyFileCommand(string command, string expectedContents)
        {
            var filePath = Path.Combine(TestingUtils.GetAssemblyDirectory(), "test", command);
            var contents = File.ReadAllText(filePath, Encoding.UTF8);

            try
            {
                Assert.AreEqual(expectedContents, contents);
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
