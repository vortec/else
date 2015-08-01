using System.Linq;
using Autofac.Extras.Moq;
using Else.Core;
using Else.Extensibility;
using Moq;
using NUnit.Framework;

namespace Else.Tests
{
    [TestFixture]
    internal class EngineTest
    {
        [TestCase]
        public async void CommandPluginTest()
        {
            using (var mock = AutoMock.GetLoose()) {
                // create mock AppCommands (so we can test that the plugin calls it)
                mock.Mock<IAppCommands>().Setup(x => x.HideWindow());

                // create Engine and register test plugin
                var engine = mock.Create<Engine>();
                var plugin = new CommandPlugin();
                plugin.Setup();
                plugin.AppCommands = mock.Mock<IAppCommands>().Object;
                engine.LoadedPlugins.Add(plugin);

                // send engine a query and wait for it to finish
                await engine.BeginQuery("command1");

                // verify that TestPlugin returned the expected result
                var results = engine.ResultsList.Where(r => r.Title == "Command 1");
                Assert.AreEqual(1, results.Count());
                var result = results.First();

                // verify the result data
                Assert.AreEqual("Command 1", result.Title);
                Assert.AreEqual("Command 1 SubTitle", result.SubTitle);
                Assert.IsTrue(result.HasSubTitle);
                Assert.AreEqual(0, result.Index);

                // call the launch action of the result
                result.Launch(engine.Query);

                // verify the launch action was called once
                mock.Mock<IAppCommands>().Verify(x => x.HideWindow(), Times.Once);
            }
        }
    }

    public class CommandPlugin : Plugin
    {
        public override void Setup()
        {
            // register a single basic command
            AddCommand("command1")
                .Title("Command 1")
                .Subtitle("Command 1 SubTitle")
                .Launch(query => AppCommands.HideWindow());
        }
    }
}