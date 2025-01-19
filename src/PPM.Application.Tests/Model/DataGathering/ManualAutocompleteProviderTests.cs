using System.Collections.Generic;
using System.Linq;
using Affinity_manager.Model.DataGathering;
using NUnit.Framework;

namespace PPM.Application.Tests.Model.DataGathering
{
    [TestFixture]
    public class ManualAutocompleteProviderTests
    {
        private ManualAutocompleteProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _provider = new ManualAutocompleteProvider();
        }

        [Test]
        public void AddProcesses_ShouldAddValidProcesses()
        {
            // Arrange
            List<string> processes = ["Process1", "Process2", "  ", "Process3"];

            // Act
            _provider.AddProcesses(processes);

            // Assert
            List<ProcessInfo> addedProcesses = _provider.GetMatchedProcesses(string.Empty).ToList();
            Assert.That(addedProcesses, Has.Count.EqualTo(3));
            Assert.That(addedProcesses.Any(p => p.MainModuleName == "Process1"), Is.True);
            Assert.That(addedProcesses.Any(p => p.MainModuleName == "Process2"), Is.True);
            Assert.That(addedProcesses.Any(p => p.MainModuleName == "Process3"), Is.True);
            Assert.That(addedProcesses.Select(p => p.Source), Has.All.EqualTo(Source.ExistingProfiles));
        }

        [Test]
        public void GetMatchedProcesses_ShouldReturnMatchingProcesses()
        {
            // Arrange
            List<string> processes = new()
            { "Process1", "Process2", "TestProcess", "AnotherProcess" };
            _provider.AddProcesses(processes);

            // Act
            List<ProcessInfo> matchedProcesses = _provider.GetMatchedProcesses("Process").ToList();

            // Assert
            Assert.That(matchedProcesses.Count, Is.EqualTo(2));
            Assert.That(matchedProcesses.Any(p => p.MainModuleName == "Process1"), Is.True);
            Assert.That(matchedProcesses.Any(p => p.MainModuleName == "Process2"), Is.True);
        }

        [Test]
        public void GetMatchedProcesses_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            List<string> processes = new()
            { "Process1", "Process2", "TestProcess" };
            _provider.AddProcesses(processes);

            // Act
            List<ProcessInfo> matchedProcesses = _provider.GetMatchedProcesses("NonExistent").ToList();

            // Assert
            Assert.That(matchedProcesses, Is.Empty);
        }
    }
}
