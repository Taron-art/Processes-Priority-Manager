using System.Collections.Generic;
using Affinity_manager.ViewWrappers;
using FluentAssertions;
using FluentAssertions.Events;
using NUnit.Framework;

namespace PPM.Application.Tests.ViewWrappers
{
    [TestFixture]
    public class AffinityViewTests
    {
        private AffinityView _affinityView;

        [SetUp]
        public void SetUp()
        {
            _affinityView = new AffinityView(0b1010, 4);
        }

        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            Assert.That(_affinityView.AffinityMask, Is.EqualTo(0b1010));
            Assert.That(_affinityView.LogicalCpus.Count, Is.EqualTo(4));
        }

        private static IEnumerable<TestCaseData<ulong, string>> FriendlyViewTestData()
        {
            yield return new TestCaseData<ulong, string>(0b1010, $"{string.Format(Affinity_manager.Strings.PPM.CpuFormat, 1)}, {string.Format(Affinity_manager.Strings.PPM.CpuFormat, 3)}");
            yield return new TestCaseData<ulong, string>(0b1100, $"{string.Format(Affinity_manager.Strings.PPM.CpuFormat, 2)}, {string.Format(Affinity_manager.Strings.PPM.CpuFormat, 3)}");
            yield return new TestCaseData<ulong, string>(0b1111, Affinity_manager.Strings.PPM.AllCpus);
        }

        [TestCaseSource(nameof(FriendlyViewTestData))]
        public void FriendlyView_ShouldReturnCorrectString(ulong affinity, string expectedFriendlyView)
        {
            _affinityView.UpdateAffinityMask(affinity);
            Assert.That(_affinityView.FriendlyView, Is.EqualTo(expectedFriendlyView));
        }

        [TestCase(0b1010ul, false)]
        [TestCase(0b1011ul, false)]
        [TestCase(0b1111ul, true)]
        public void AllCpus_ShoudReturnIfAllCoresAreSelected(ulong affinity, bool expected)
        {
            _affinityView.UpdateAffinityMask(affinity);
            Assert.That(_affinityView.AllCpus, Is.EqualTo(expected));
        }

        [Test]
        public void AllCpus_ShouldReturnFalse_WhenNotAllCoresAreEnabled()
        {
            Assert.That(_affinityView.AllCpus, Is.False);
        }

        [Test]
        public void CanAccept_ShouldReturnTrue_WhenAtLeastOneCoreIsEnabled()
        {
            Assert.That(_affinityView.CanAccept, Is.True);
        }

        [Test]
        public void CanAccept_ShouldReturnFalse_WhenNoCoresAreEnabled()
        {
            _affinityView.AllCpus = false;
            Assert.That(_affinityView.CanAccept, Is.False);
        }

        [Test]
        public void UpdateAffinityMask_ShouldUpdateAffinityMask()
        {
            _affinityView.UpdateAffinityMask(0b1100);
            Assert.That(_affinityView.AffinityMask, Is.EqualTo(0b1100));
        }

        [Test]
        public void ApplyChanges_ShouldUpdateAffinityMask()
        {
            _affinityView.LogicalCpus[0].Value = true;
            _affinityView.ApplyChanges();
            Assert.That(_affinityView.AffinityMask - (ulong.MaxValue << 4), Is.EqualTo(0b1011));
        }

        [Test]
        public void CancelChanges_ShouldRevertAffinityMask()
        {
            _affinityView.LogicalCpus[0].Value = true;
            _affinityView.CancelChanges();
            Assert.That(_affinityView.AffinityMask, Is.EqualTo(0b1010));
        }

        [Test]
        public void PropertyChanged_ShouldBeRaised_WhenAffinityMaskChanges()
        {
            using IMonitor<AffinityView> monitor = _affinityView.Monitor();
            _affinityView.UpdateAffinityMask(0b1100);

            monitor.Should().RaisePropertyChangeFor((view) => view.FriendlyView);
            monitor.Should().RaisePropertyChangeFor((view) => view.AllCpus);
            monitor.Should().RaisePropertyChangeFor((view) => view.CanAccept);
        }

        [Test]
        public void PropertyChanged_ShouldBeRaised_WhenCoreViewIsChanged()
        {
            using IMonitor<AffinityView> monitor = _affinityView.Monitor();
            _affinityView.LogicalCpus[0].Value = true;
            monitor.Should().RaisePropertyChangeFor((view) => view.AllCpus);
            monitor.Should().RaisePropertyChangeFor((view) => view.CanAccept);
        }
    }
}
