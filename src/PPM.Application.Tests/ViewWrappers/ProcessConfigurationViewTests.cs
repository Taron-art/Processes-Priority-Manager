using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Affinity_manager.Model;
using Affinity_manager.ViewWrappers;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Events;
using NUnit.Framework;

namespace PPM.Application.Tests.ViewWrappers
{
    [TestFixture]
    public class ProcessConfigurationViewTests
    {
        private ProcessConfigurationView _view;
        private ProcessConfiguration _configuration;
        private IOptionsProvider _optionsProvider;

        [SetUp]
        public void SetUp()
        {
            _configuration = new("TestName.exe");
            _optionsProvider = A.Fake<IOptionsProvider>(options => options.Strict());
            A.CallTo(() => _optionsProvider.NumberOfLogicalCpus).Returns(5U);

            List<EnumViewWrapper<CpuPriorityClass>> priorityClasses = new();
            A.CallTo(() => _optionsProvider.CpuPriorities).Returns(priorityClasses);

            List<EnumViewWrapper<IoPriority>> ioPriorities = new();
            A.CallTo(() => _optionsProvider.IoPriorities).Returns(ioPriorities);
            _view = new ProcessConfigurationView(_configuration, _optionsProvider);
        }

        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            Assert.That(_view.ProcessConfiguration, Is.SameAs(_configuration));
            Assert.That(_view.OptionsProvider, Is.SameAs(_optionsProvider));
            Assert.That(_view.AffinityView, Is.Not.Null);
            Assert.That(_view.AffinityView.AffinityMask, Is.EqualTo(_view.ProcessConfiguration.CpuAffinityMask));

            _view = new ProcessConfigurationView(_configuration, _optionsProvider);
            Assert.That(_view.CpuPriorities, Is.SameAs(_optionsProvider.CpuPriorities));
            Assert.That(_view.IoPriorities, Is.SameAs(_optionsProvider.IoPriorities));
            Assert.That(_view.AffinityView.LogicalCpus.Count, Is.EqualTo(5));
        }

        [Test]
        public void Name_ShouldReturnConfigurationName()
        {
            Assert.That(_view.Name, Is.SameAs(_configuration.Name));
        }

        [Test]
        public void CpuAffinityMask_ShouldReturnConfigurationCpuAffinityMask()
        {
            _configuration.CpuAffinityMask = 12345UL;
            Assert.That(_view.CpuAffinityMask, Is.EqualTo(_configuration.CpuAffinityMask));
        }

        [Test]
        public void IsDirty_ShouldBeFalseInitially()
        {
            Assert.That(_view.IsDirty, Is.False);
        }

        [Test]
        public void MarkDirty_ShouldSetIsDirtyToTrue()
        {
            _view.MarkDirty();
            Assert.That(_view.IsDirty, Is.True);
        }

        public static IEnumerable<TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object>>>> SetActions()
        {
            // Year, it is complex, but what a person should do if he/she wants to avoid copy-paste?
            // This method returns a collection of a set delegate that sets the property and the Expression to check the property change.
            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object>>>(
                (ProcessConfiguration configuration) => configuration.CpuPriority = CpuPriorityClass.High, view => view.CpuPriority)
                .SetArgDisplayNames(nameof(ProcessConfiguration.CpuPriority), nameof(ProcessConfiguration.CpuPriority));

            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object>>>(
                (ProcessConfiguration configuration) => configuration.IoPriority = IoPriority.Low, view => view.IoPriority)
                .SetArgDisplayNames(nameof(ProcessConfiguration.IoPriority), nameof(ProcessConfiguration.IoPriority));

            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object>>>(
                (ProcessConfiguration configuration) => configuration.CpuAffinityMask = 1234U, view => view.CpuAffinityMask)
                .SetArgDisplayNames(nameof(ProcessConfiguration.CpuAffinityMask), nameof(ProcessConfiguration.CpuAffinityMask));
        }

        [TestCaseSource(nameof(SetActions))]
        public void OnProcessConfigurationPropertyChanged_ShouldSetIsDirtyToTrue(Action<ProcessConfiguration> setAction, object _)
        {
            setAction(_view.ProcessConfiguration);
            Assert.That(_view.IsDirty, Is.True);
        }

        [TestCaseSource(nameof(SetActions))]
        public void OnProcessConfigurationPropertyChanged_ShouldRaisePropertyChanged(Action<ProcessConfiguration> setAction, Expression<Func<ProcessConfigurationView, object>> expectedExpression)
        {
            using IMonitor<ProcessConfigurationView> monitor = _view.Monitor();
            setAction(_view.ProcessConfiguration);

            monitor.Should().RaisePropertyChangeFor(expectedExpression);
            monitor.Should().RaisePropertyChangeFor((view) => view.IsDirty);
        }

        [Test]
        public void OnAffinityViewAffinityMaskChange_ShouldUpdateCpuAffinityMask()
        {
            _view.AffinityView.AffinityMask = 1234U;
            Assert.That(_view.CpuAffinityMask, Is.EqualTo(_view.AffinityView.AffinityMask));
        }
    }
}
