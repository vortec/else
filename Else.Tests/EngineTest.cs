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
        [Test]
        public async void TestEngineReturnsExpectedResults()
        {
            using (var mock = AutoMock.GetLoose()) {
                // create Engine
                var engine = mock.Create<Engine>();

                // create and register our mock plugin
                var plugin = new CommandPlugin();
                plugin.Setup();
                engine.LoadedPlugins.Add(plugin);

                // send engine a query and wait for it to finish
                await engine.BeginQuery("command1");

                // verify that TestPlugin returned the expected result
                var results = engine.Results.Where(r => r.Title == "Command 1").ToList();
                Assert.That(results.Count(), Is.EqualTo(1));
                var result = results.First();

                // verify the result data
                Assert.That("Command 1", Is.EqualTo(result.Title));
                Assert.That("Command 1 SubTitle", Is.EqualTo(result.SubTitle));
                Assert.That(result.HasSubTitle, Is.True);
                Assert.That(result.Index, Is.EqualTo(0));
            }
        }

        // this test should be moved to Plugin or ResultProvider
        [Test]
        public async void TestLaunchAction()
        {
            using (var mock = AutoMock.GetLoose()) {
                // create mock AppCommands (so we can test that the plugin calls it)
                mock.Mock<IAppCommands>().Setup(x => x.HideWindow());

                // create Engine
                var engine = mock.Create<Engine>();

                // create and register our mock plugin
                var plugin = new CommandPlugin();
                // inject our mock AppCommands
                plugin.AppCommands = mock.Mock<IAppCommands>().Object;
                plugin.Setup();
                engine.LoadedPlugins.Add(plugin);

                // send engine a query and wait for it to finish
                await engine.BeginQuery("command1");
                
                var results = engine.Results; // the query results

                // find the expected result in the list
                var filteredResults = results.Where(r => r.Title == "Command 1").ToList();

                // CommandPlugin should only return 1 result
                Assert.That(filteredResults.Count(), Is.EqualTo(1));

                // call the launch action of the result
                var result = filteredResults.First();
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