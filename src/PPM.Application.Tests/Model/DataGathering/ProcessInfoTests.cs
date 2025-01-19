using System.Collections.Generic;
using Affinity_manager.Model.DataGathering;
using NUnit.Framework;

namespace PPM.Application.Tests.Model.DataGathering
{
    [TestFixture]
    public class ProcessInfoTests
    {
        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            string mainModuleName = "TestModule";
            Source source = Source.RunningTasks;

            ProcessInfo processInfo = new(mainModuleName, source);

            Assert.That(processInfo.MainModuleName, Is.EqualTo(mainModuleName));
            Assert.That(processInfo.Source, Is.EqualTo(source));
            Assert.That(processInfo.ModuleFullPath, Is.Null);
        }

        [Test]
        public void Matches_ShouldReturnTrue_WhenPrefixMatchesMainModuleName()
        {
            ProcessInfo processInfo = new("TestModule");

            Assert.That(processInfo.Matches("Test"));
        }

        [Test]
        public void Matches_ShouldReturnFalse_WhenPrefixDoesNotMatch()
        {
            ProcessInfo processInfo = new("TestModule");

            Assert.That(processInfo.Matches("NonMatchingPrefix"), Is.False);
        }

        [Test]
        public void UpdateWithFriendlyNameAndModulePath_ShouldUpdateProperties()
        {
            // Arrange
            ProcessInfo processInfo = new("TestModule");
            string friendlyName = "FriendlyName";
            string modulePath = "ModulePath";

            // Act
            processInfo.UpdateWithFriendlyNameAndModulePath(friendlyName, modulePath);

            // Assert
            Assert.That(processInfo.FriendlyName, Is.EqualTo(friendlyName));
            Assert.That(processInfo.ModuleFullPath, Is.EqualTo(modulePath));
        }

        [TestCase("TestModule", "TestModule", true)]
        [TestCase("Testmodule", "TestModule", true)]
        [TestCase("TestModule1", "TestModule", false)]
        public void Equals_ShouldReturnTrueOrFalseDependingOnMainModuleName(string name1, string name2, bool expectedValue)
        {
            // Arrange
            ProcessInfo processInfo1 = new(name1, (Source)TestContext.CurrentContext.Random.Next(0, 2));
            ProcessInfo processInfo2 = new(name2, (Source)TestContext.CurrentContext.Random.Next(0, 2));

            Assert.That(processInfo1.Equals(processInfo2), Is.EqualTo(expectedValue));
        }

        [TestCase("TestModule", "TestModule", true)]
        [TestCase("Testmodule", "TestModule", true)]
        [TestCase("TestModule1", "TestModule", false)]
        public void GetHashCode_ShouldReturnSameHashCode_ForEqualObjects(string name1, string name2, bool expectedEqual)
        {
            ProcessInfo processInfo1 = new(name1, (Source)TestContext.CurrentContext.Random.Next(0, 2));
            ProcessInfo processInfo2 = new(name2, (Source)TestContext.CurrentContext.Random.Next(0, 2));

            int hashCode1 = processInfo1.GetHashCode();
            int hashCode2 = processInfo2.GetHashCode();

            Assert.That(hashCode2, expectedEqual ? Is.EqualTo(hashCode1) : Is.Not.EqualTo(hashCode1));
        }

        [TestCase("Test1", "Test1", 0)]
        [TestCase("test1", "Test1", 0)]
        [TestCase("TestModuleB", "TestModuleA", 1)]
        [TestCase("TestModuleB", "TestModulec", -1)]
        public void CompareTo_ShouldReturnDependingFromMainModuleName(string name1, string name2, int expectedValue)
        {
            ProcessInfo processInfo1 = new(name1);
            ProcessInfo processInfo2 = new(name2);

            int result = processInfo1.CompareTo(processInfo2);

            Assert.That(result, Is.EqualTo(expectedValue));
        }

        private static IEnumerable<TestCaseData> FriendlyNameTestCases
        {
            get
            {
                yield return new TestCaseData(Source.None, null);
                yield return new TestCaseData(Source.RunningTasks, Affinity_manager.Strings.PPM.BackgroundProcess);
                yield return new TestCaseData(Source.ExistingProfiles, Affinity_manager.Strings.PPM.FromSavedProfile);
            }
        }

        [TestCaseSource(nameof(FriendlyNameTestCases))]
        [SetUICulture("en-US")]
        public void FriendlyName_ShouldReturnExpectedValue(Source source, string? expectedFriendlyName)
        {
            ProcessInfo processInfo = new("TestModule", source);

            Assert.That(processInfo.FriendlyName, Is.EqualTo(expectedFriendlyName));
        }
    }
}
