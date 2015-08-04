using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using Else.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Else.Tests
{
    [TestFixture]
    internal class ThemeObjectTest
    {
        private readonly Dictionary<string, string> _themeDict = new Dictionary<string, string>
        {
            {"Name", "Light"},
            {"Author", "James Hutchby"},
            {"WindowBorderColor", "#7F363636"},
            {"WindowBackgroundColor", "White"},
            {"QueryBoxBackgroundColor", "White"},
            {"QueryBoxTextColor", "Black"},
            {"ResultBackgroundColor", "White"},
            {"ResultSelectedBackgroundColor", "#EEEDEF"},
            {"ResultTitleColor", "Black"},
            {"ResultSubTitleColor", "#777777"},
            {"GUID", "Light"}
        };

        private readonly string[] _colorKeys =
        {
            "WindowBorderColor", "WindowBackgroundColor", "QueryBoxBackgroundColor", "QueryBoxTextColor",
            "ResultBackgroundColor", "ResultSelectedBackgroundColor", "ResultTitleColor", "ResultSubTitleColor"
        };

        private string _themePath;

        [SetUp]
        public void Setup()
        {
            // serialize _themeDict to json file _themePath
            _themePath = Path.GetTempFileName();
            File.WriteAllText(_themePath, JsonConvert.SerializeObject(_themeDict));
        }

        [TearDown]
        public void Dispose()
        {
            // delete temporary file
            File.Delete(_themePath);
        }

        [Test]
        public void TestLoad()
        {
            var theme = new Theme();
            theme.LoadFromPath(_themePath);

            // Assert the loaded theme has expected values
            foreach (var field in _themeDict) {
                Assert.That(theme.Config[field.Key], Is.EqualTo(field.Value));
            }
        }

        [Test]
        public void TestLoadParseException()
        {
            // create new file
            var corruptThemePath = Path.GetTempFileName();

            // load the test theme file, and write a corrupt version
            var contents = File.ReadAllText(_themePath);
            contents = contents.Replace("{", "!!!");
            File.WriteAllText(corruptThemePath, contents);

            // assert that loading causes exception
            Assert.That(() =>
            {
                var theme = new Theme();
                theme.LoadFromPath(corruptThemePath);
            }, Throws.Exception.TypeOf<JsonReaderException>());
        }

        [Test]
        public void TestLoadWithBadPath()
        {
            var theme = new Theme();
            var randomPath = Path.Combine("c:\\", Path.GetRandomFileName());
            Assert.Throws<FileNotFoundException>(() => theme.LoadFromPath(randomPath));
        }

        [Test]
        public void TestDelete()
        {
            // copy test theme
            var copyPath = Path.GetTempFileName();
            File.Copy(_themePath, copyPath, true);

            // load the copy
            var theme = new Theme();
            theme.LoadFromPath(copyPath);

            // delete
            theme.Delete();

            // assert the file does not exist
            Assert.That(File.Exists(copyPath), Is.False);
        }

        [Test]
        public void TestResourceDictionary()
        {
            var theme = new Theme();
            theme.LoadFromPath(_themePath);

            var resourceDictionary = theme.ToResourceDictionary();
            // test the resource dictionary for correctness by comparing each color
            foreach (var colorKey in _colorKeys) {
                var actualColor = ((SolidColorBrush) resourceDictionary[colorKey]).Color;
                var expectedColor =
                    ((SolidColorBrush) new BrushConverter().ConvertFromString(_themeDict[colorKey])).Color;
                Assert.That(actualColor, Is.EqualTo(expectedColor));
            }
        }

        [Test]
        public void TestSave()
        {
            // load test theme
            var theme = new Theme();
            theme.LoadFromPath(_themePath);

            // export to another path
            var themeSavePath = Path.GetTempFileName();
            theme.Save(themeSavePath);

            // load the export
            var exportedTheme = new Theme();
            exportedTheme.LoadFromPath(themeSavePath);

            // Assert the loaded theme has expected values
            foreach (var field in _themeDict) {
                Assert.That(theme.Config[field.Key], Is.EqualTo(field.Value));
            }
        }

        //public void TestClone()
        //{
        //}

        //public void TestDuplicate()
        //{
        //}
        //public void TestCopyFrom()
        //{
        //}
    }
}