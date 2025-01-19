using System;
using Affinity_manager.Model.DataGathering;
using NUnit.Framework;
using ShellLink;
using ShellLink.Structures;

namespace PPM.Application.Tests.Model.DataGathering
{
    [TestFixture]
    public class ShortcutExtensionsTests
    {
        [Test]
        public void GetExeTargetFullPath_ValidExePath_ReturnsFullPath()
        {
            // Arrange
            Shortcut shortcut = new()
            {
                LinkTargetIDList = new LinkTargetIDList { Path = "C:\\Program Files\\Example\\example.exe" }
            };

            // Act
            string? result = shortcut.GetExeTargetFullPath();

            // Assert
            Assert.That(result, Is.EqualTo(@"C:\Program Files\Example\example.exe"));
        }

        [Test]
        public void GetExeTargetFullPath_InvalidExePath_ReturnsNull()
        {
            // Arrange
            Shortcut shortcut = new()
            {
                LinkTargetIDList = new LinkTargetIDList { Path = @"C:\Program Files\Example\example.txt" }
            };

            // Act
            string? result = shortcut.GetExeTargetFullPath();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetIconPath_InvalidIconPath_ReturnsExePath()
        {
            // Arrange
            Shortcut shortcut = new()
            {
                StringData = new StringData { IconLocation = @"%SystemRoot%\system32\SHELL32.dll,0" },
                LinkTargetIDList = new LinkTargetIDList { Path = @"C:\Program Files\Example\example.exe" }
            };

            // Act
            string? result = shortcut.GetIconPath();

            // Assert
            Assert.That(result, Is.EqualTo(@"C:\Program Files\Example\example.exe"));
        }
    }
}
