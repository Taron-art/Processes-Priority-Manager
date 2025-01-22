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

            List<EnumViewWrapper<CpuPriorityClass>> priorityClasses = [];
            A.CallTo(() => _optionsProvider.CpuPriorities).Returns(priorityClasses);

            List<EnumViewWrapper<IoPriority>> ioPriorities = [];
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
            Assert.That(_view.AffinityView.LogicalCpus, Has.Count.EqualTo(5));
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

        private static IEnumerable<TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>> SetActions()
        {
            // Year, it is complex, but what a person should do if he/she wants to avoid copy-paste?
            // This method returns a collection of a set delegate that sets the property and the Expression to check the property change.
            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>(
                    (ProcessConfiguration configuration) => configuration.CpuPriority = CpuPriorityClass.High,
                    view => view.CpuPriority)
                .SetArgDisplayNames(nameof(ProcessConfiguration.CpuPriority), nameof(ProcessConfigurationView.CpuPriority));

            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>(
                    (ProcessConfiguration configuration) => configuration.IoPriority = IoPriority.Low,
                    view => view.IoPriority)
                .SetArgDisplayNames(nameof(ProcessConfiguration.IoPriority), nameof(ProcessConfigurationView.IoPriority));

            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>(
                    (ProcessConfiguration configuration) => configuration.IoPriority = IoPriority.Low,
                    view => view.MemoryAndIoPrioritiesAreLowest)
                .SetArgDisplayNames(nameof(ProcessConfiguration.IoPriority), nameof(ProcessConfigurationView.MemoryAndIoPrioritiesAreLowest));

            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>(
                    (ProcessConfiguration configuration) => configuration.CpuAffinityMask = 1234U,
                    view => view.CpuAffinityMask)
                .SetArgDisplayNames(nameof(ProcessConfiguration.CpuAffinityMask), nameof(ProcessConfigurationView.CpuAffinityMask));

            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>(
                    (ProcessConfiguration configuration) => configuration.MemoryPriority = PagePriority.VeryLow,
                    view => view.MemoryPriority)
                .SetArgDisplayNames(nameof(ProcessConfiguration.MemoryPriority), nameof(ProcessConfigurationView.MemoryPriority));

            yield return (TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>)
                new TestCaseData<Action<ProcessConfiguration>, Expression<Func<ProcessConfigurationView, object?>>>(
                    (ProcessConfiguration configuration) => configuration.MemoryPriority = PagePriority.VeryLow,
                    view => view.MemoryAndIoPrioritiesAreLowest)
                .SetArgDisplayNames(nameof(ProcessConfiguration.MemoryPriority), nameof(ProcessConfigurationView.MemoryAndIoPrioritiesAreLowest));
        }

        [TestCaseSource(nameof(SetActions))]
        public void OnProcessConfigurationPropertyChanged_ShouldSetIsDirtyToTrue(Action<ProcessConfiguration> setAction, object _)
        {
            setAction(_view.ProcessConfiguration);
            Assert.That(_view.IsDirty, Is.True);
        }

        [TestCaseSource(nameof(SetActions))]
        public void OnProcessConfigurationPropertyChanged_ShouldRaisePropertyChanged(Action<ProcessConfiguration> setAction, Expression<Func<ProcessConfigurationView, object?>> expectedExpression)
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

        [TestCase(PagePriority.VeryLow, IoPriority.VeryLow, true)]
        [TestCase(PagePriority.Low, IoPriority.VeryLow, null)]
        [TestCase(PagePriority.Normal, IoPriority.VeryLow, null)]
        [TestCase(PagePriority.BelowNormal, IoPriority.VeryLow, null)]
        [TestCase(PagePriority.Medium, IoPriority.VeryLow, null)]
        [TestCase(PagePriority.VeryLow, IoPriority.Low, null)]
        [TestCase(PagePriority.VeryLow, IoPriority.Normal, null)]
        [TestCase(PagePriority.Medium, IoPriority.Low, false)]
        [TestCase(PagePriority.Normal, IoPriority.Normal, false)]
        [TestCase(PagePriority.Low, IoPriority.Normal, false)]
        public void MemoryAndIoPrioritiesAreLowest_ReturnValue(PagePriority memoryPriority, IoPriority ioPriority, bool? expectedValue)
        {
            _configuration.MemoryPriority = memoryPriority;
            _configuration.IoPriority = ioPriority;

            Assert.That(_view.MemoryAndIoPrioritiesAreLowest, Is.EqualTo(expectedValue));
        }

        [Test]
        public void MemoryAndIoPrioritiesAreLowest_SetToTrue_ShouldSetBothToVeryLow()
        {
            _view.MemoryAndIoPrioritiesAreLowest = true;

            Assert.That(_configuration.MemoryPriority, Is.EqualTo(PagePriority.VeryLow));
            Assert.That(_configuration.IoPriority, Is.EqualTo(IoPriority.VeryLow));
        }

        [Test]
        public void MemoryAndIoPrioritiesAreLowest_SetToFalse_ShouldResetBothToDefault()
        {
            _configuration.MemoryPriority = PagePriority.VeryLow;
            _configuration.IoPriority = IoPriority.VeryLow;

            _view.MemoryAndIoPrioritiesAreLowest = false;

            Assert.That(_configuration.MemoryPriority, Is.EqualTo(PagePriority.Normal));
            Assert.That(_configuration.IoPriority, Is.EqualTo(IoPriority.Normal));
        }

        [Test]
        public void ToolTip_ShouldReturnNullIfConfigurationIsNotDirty()
        {
            Assert.That(_view.ToolTip, Is.Null);
        }

        [Test]
        public void ToolTip_ShouldReturnDescriptionIfConfigurationIsDirtyButNotEmpty()
        {
            _configuration.IoPriority = IoPriority.Low;

            string expectedToolTip = $"{Affinity_manager.Strings.PPM.ProcessorConfigurationModifiedToolTipStart} {Affinity_manager.Strings.PPM.ProcessorConfigurationModifiedToolTipEnd}";
            Assert.That(_view.ToolTip, Is.EqualTo(expectedToolTip));
        }

        [Test]
        public void ToolTip_ShouldReturnDifferentDescriptionIfConfigurationIsDirtyAndEmpty()
        {
            _configuration.Reset();
            _view.MarkDirty();

            string expectedToolTip = $"{Affinity_manager.Strings.PPM.ProcessorConfigurationModifiedToolTipStart} {Affinity_manager.Strings.PPM.ProcessorConfigurationModifiedToolTipEndDefaultState}";
            Assert.That(_view.ToolTip, Is.EqualTo(expectedToolTip));
        }
    }
}
