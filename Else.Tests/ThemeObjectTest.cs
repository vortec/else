using System;
using System.IO;
using System.Runtime.Versioning;
using Else.Model;
using Newtonsoft.Json;
using NUnit.Framework;


namespace Else.Tests
{
    [TestFixture]
    internal class ThemeObjectTest
    {
        private string _themePath;
        private string _testThemePath;
        [SetUp]
        public void Setup()
        {
            // copy predefined theme (json file), to a temporary file
            var startupPath = Environment.CurrentDirectory;
            _testThemePath = Path.Combine(startupPath, "Data\\test_theme.json");
            _themePath = Path.GetTempFileName();
            File.Copy(_testThemePath, _themePath, true);
        }

        [TearDown]
        public void Dispose()
        {
            // delete temporary file
            File.Delete(_themePath);
        }

        [Test]
        public void TestLoadTheme()
        {
            var theme = new Theme();
            theme.LoadFromPath(_themePath);
            Assert.That(theme.Name, Is.EqualTo("Light"));
            Assert.That(theme.Author, Is.EqualTo("by James Hutchby"));
            Assert.That(theme.Config["WindowBorderColor"], Is.EqualTo("#7F363636"));
            Assert.That(theme.Config["WindowBackgroundColor"], Is.EqualTo("White"));
            Assert.That(theme.Config["QueryBoxBackgroundColor"], Is.EqualTo("White"));
            Assert.That(theme.Config["QueryBoxTextColor"], Is.EqualTo("Black"));
            Assert.That(theme.Config["ResultBackgroundColor"], Is.EqualTo("White"));
            Assert.That(theme.Config["ResultSelectedBackgroundColor"], Is.EqualTo("#EEEDEF"));
            Assert.That(theme.Config["ResultTitleColor"], Is.EqualTo("Black"));
            Assert.That(theme.Config["ResultSubTitleColor"], Is.EqualTo("#777777"));
            Assert.That(theme.Config["GUID"], Is.EqualTo("Light"));
        }

        [Test]
        public void TestLoadWithBadPath()
        {
            var theme = new Theme();
            var randomPath = Path.Combine("c:\\", Path.GetRandomFileName());
            Assert.Throws<FileNotFoundException>(() => theme.LoadFromPath(randomPath));

        }

        [Test]
        public void TestLoadParseException()
        {
            // create new file
            var corruptThemePath = Path.GetTempFileName();
            
            // load the test theme file, and write a corrupt version
            var contents = File.ReadAllText(_testThemePath);
            contents = contents.Replace("{", "!!!");
            File.WriteAllText(corruptThemePath, contents);

            // assert that loading causes exception
            Assert.That(() =>
            {
                var theme = new Theme();
                theme.LoadFromPath(corruptThemePath);
            }, Throws.Exception.TypeOf<JsonReaderException>());
        }
        
        // todo:
        // test save
        // test delete
        // test resource dictionary
        // test clone
        // test duplicate
        // test copy from

    }
}