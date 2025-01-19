using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Affinity_manager.Model.DataGathering;
using NUnit.Framework;
using ShellLink;
using ShellLink.Structures;
using static System.Environment;

namespace PPM.Application.IntegrationTests.Model.DataGathering
{
    [TestFixture(SpecialFolder.StartMenu)]
    [TestFixture(SpecialFolder.CommonStartMenu)]
    public class StartMenuShortcutsGathererTests
    {
        private const string TestShortcutName = "TestShortcut.lnk";
        private const string TestShortcutTarget = @"C:\Windows\System32\notepad.exe";

        private readonly string _startMenuPath;

        public SpecialFolder SpecialFolder { get; }

        public StartMenuShortcutsGathererTests(SpecialFolder specialFolder)
        {
            _startMenuPath = GetFolderPath(specialFolder);
            SpecialFolder = specialFolder;
        }

        [SetUp]
        public void SetUp()
        {
            CreateTestShortcut(_startMenuPath);
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTestShortcut(_startMenuPath);
        }

        [Test]
        public async Task StartMenuShortcutsGatherer_FindsShortcuts()
        {
            // Arrange
            StartMenuShortcutsGatherer gatherer = new();
            await gatherer.CollectAsync();

            // Act
            List<ProcessInfo> matchedProcesses = gatherer.GetMatchedProcesses("notepad.exe").ToList();

            // Assert
            Assert.That(matchedProcesses, Is.Not.Null);
            Assert.That(matchedProcesses, Has.Count.EqualTo(1));
            Assert.That(matchedProcesses[0].FriendlyName, Is.EqualTo(Path.GetFileNameWithoutExtension(TestShortcutName)));
            Assert.That(matchedProcesses[0].MainModuleName, Is.EqualTo(Path.GetFileName(TestShortcutTarget)));
            Assert.That(matchedProcesses[0].Source, Is.EqualTo(Source.None));
        }

        private void CreateTestShortcut(string startMenuPath)
        {
            string shortcutPath = Path.Combine(startMenuPath, TestShortcutName);
            if (!Directory.Exists(startMenuPath))
            {
                Directory.CreateDirectory(startMenuPath);
            }
            Shortcut shortcut = new();
            shortcut.LinkTargetIDList = new LinkTargetIDList() { Path = TestShortcutTarget };
            shortcut.StringData = new StringData() { IconLocation = TestShortcutTarget };
            shortcut.WriteToFile(shortcutPath);
        }

        private void DeleteTestShortcut(string startMenuPath)
        {
            string shortcutPath = Path.Combine(startMenuPath, TestShortcutName);
            if (File.Exists(shortcutPath))
            {
                File.Delete(shortcutPath);
            }
        }
    }
}
