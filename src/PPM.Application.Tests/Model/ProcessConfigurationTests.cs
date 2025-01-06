using System;
using System.Collections.Generic;
using Affinity_manager.Model;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace PPM.Application.Tests.Model
{
    [TestFixture]
    public class ProcessConfigurationTests
    {
        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string name = "TestProcess";

            // Act
            ProcessConfiguration config = new(name);

            // Assert
            Assert.That(config.Name, Is.EqualTo(name));
            Assert.That(config.CpuAffinityMask, Is.EqualTo(ProcessConfiguration.AffinityDefaultValue));
            Assert.That(config.CpuPriority, Is.EqualTo(ProcessConfiguration.CpuPriorityDefaultValue));
            Assert.That(config.IoPriority, Is.EqualTo(ProcessConfiguration.IoPriorityDefaultValue));
        }

        [Test]
        public void IsEmpty_ShouldReturnTrue_WhenAllPropertiesAreDefault()
        {
            // Arrange
            ProcessConfiguration config = new("TestProcess");

            // Act
            bool isEmpty = config.IsEmpty;

            // Assert
            ClassicAssert.IsTrue(isEmpty);
        }

        public static IEnumerable<TestCaseData> SetActions()
        {
            yield return new TestCaseData<Action<ProcessConfiguration>>((config) => config.CpuAffinityMask = 0).SetArgDisplayNames(nameof(ProcessConfiguration.CpuAffinityMask));
            yield return new TestCaseData<Action<ProcessConfiguration>>((config) => config.CpuPriority = CpuPriorityClass.High).SetArgDisplayNames(nameof(ProcessConfiguration.CpuPriority));
            yield return new TestCaseData<Action<ProcessConfiguration>>((config) => config.IoPriority = IoPriority.Critical).SetArgDisplayNames(nameof(ProcessConfiguration.IoPriority));
        }

        [TestCaseSource(nameof(SetActions))]
        public void IsEmpty_ShouldReturnFalse_WhenAnyPropertyIsNotDefault(Action<ProcessConfiguration> setConfigurationPropertyAction)
        {
            // Arrange
            ProcessConfiguration config = new("TestProcess");
            setConfigurationPropertyAction(config);

            // Act
            bool isEmpty = config.IsEmpty;

            // Assert
            Assert.That(isEmpty, Is.False);
        }

        [Test]
        public void Reset_ShouldSetAllPropertiesToDefault()
        {
            // Arrange
            ProcessConfiguration config = new("TestProcess")
            {
                CpuAffinityMask = 0,
                CpuPriority = CpuPriorityClass.High,
                IoPriority = IoPriority.Critical
            };

            // Act
            config.Reset();

            // Assert
            Assert.That(config.CpuAffinityMask, Is.EqualTo(ProcessConfiguration.AffinityDefaultValue));
            Assert.That(config.CpuPriority, Is.EqualTo(ProcessConfiguration.CpuPriorityDefaultValue));
            Assert.That(config.IoPriority, Is.EqualTo(ProcessConfiguration.IoPriorityDefaultValue));
            Assert.That(config.IsEmpty, Is.True);
        }

        [Test]
        public void PropertyChanged_ShouldBeRaised_WhenCpuAffinityMaskChanges()
        {
            // Arrange
            ProcessConfiguration config = new("TestProcess");
            using FluentAssertions.Events.IMonitor<ProcessConfiguration> monitoredConfig = config.Monitor();

            // Act
            config.CpuAffinityMask = 0;

            // Assert
            monitoredConfig.Should().RaisePropertyChangeFor(c => c.CpuAffinityMask);
        }

        [Test]
        public void PropertyChanged_ShouldBeRaised_WhenCpuPriorityChanges()
        {
            // Arrange
            ProcessConfiguration config = new("TestProcess");
            using FluentAssertions.Events.IMonitor<ProcessConfiguration> monitoredConfig = config.Monitor();

            // Act
            config.CpuPriority = CpuPriorityClass.High;

            // Assert
            monitoredConfig.Should().RaisePropertyChangeFor(c => c.CpuPriority);
        }

        [Test]
        public void PropertyChanged_ShouldBeRaised_WhenIoPriorityChanges()
        {
            // Arrange
            ProcessConfiguration config = new("TestProcess");
            using FluentAssertions.Events.IMonitor<ProcessConfiguration> monitoredConfig = config.Monitor();

            // Act
            config.IoPriority = IoPriority.Critical;

            // Assert
            monitoredConfig.Should().RaisePropertyChangeFor(c => c.IoPriority);
        }
    }
}
