using System.Collections.Generic;
using System.Linq;
using Affinity_manager.ViewWrappers;
using Affinity_manager.ViewWrappers.Affinity;
using FluentAssertions;
using FluentAssertions.Events;
using NUnit.Framework;
using PPM.Unsafe;

namespace PPM.Application.Tests.ViewWrappers.Affinity
{
    [TestFixture]
    public class AffinityViewTests
    {
        private AffinityView _affinityView;

        [SetUp]
        public void SetUp()
        {
            List<CoreInfo> coreInfos = Enumerable.Range(0, 4).Select(i =>
            {
                CoreInfo coreInfo = new() { Id = (uint)i };
                PhysicalCoreGroup physicalCoreGroup = new() { Id = (uint)i / 2 };
                coreInfo.AddAssociatedGroup(physicalCoreGroup);
                return coreInfo;
            }).ToList();
            _affinityView = new AffinityView(0b1010, coreInfos);
        }

        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            Assert.That(_affinityView.AffinityMask, Is.EqualTo(0b1010));
            Assert.That(_affinityView.LogicalCpus.Count, Is.EqualTo(4));
        }

        private static IEnumerable<TestCaseData> FriendlyViewTestData()
        {
            yield return new TestCaseData(0b1010ul, $"{string.Format(Affinity_manager.Strings.PPM.CpuFormat, 1)}, {string.Format(Affinity_manager.Strings.PPM.CpuFormat, 3)}");
            yield return new TestCaseData(0b1100ul, $"{string.Format(Affinity_manager.Strings.PPM.CpuFormat, 2)}, {string.Format(Affinity_manager.Strings.PPM.CpuFormat, 3)}");
            yield return new TestCaseData(0b1111ul, Affinity_manager.Strings.PPM.AllCpus);
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
            _affinityView.LogicalCpus[0].Selected = true;
            _affinityView.ApplyChanges();
            Assert.That(_affinityView.AffinityMask - (ulong.MaxValue << 4), Is.EqualTo(0b1011));
        }

        [Test]
        public void CancelChanges_ShouldRevertAffinityMask()
        {
            _affinityView.LogicalCpus[0].Selected = true;
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
            _affinityView.LogicalCpus[0].Selected = true;
            monitor.Should().RaisePropertyChangeFor((view) => view.AllCpus);
            monitor.Should().RaisePropertyChangeFor((view) => view.CanAccept);
        }

        private static IEnumerable<TestCaseData> SelectSmtCoresTestData()
        {
            yield return new TestCaseData(0b0111ul, true, true);
            yield return new TestCaseData(0b0010ul, false, false);
        }

        [TestCaseSource(nameof(SelectSmtCoresTestData))]
        public void SelectSmtCores_ShouldSetAndGetCorrectValue(ulong expectedAffinity, bool value, bool expected)
        {
            _affinityView.UpdateAffinityMask(0b0010ul);
            _affinityView.SelectSmtCores = value;
            Assert.That(_affinityView.SelectSmtCores, Is.EqualTo(expected));
            _affinityView.ApplyChanges();
            Assert.That(_affinityView.AffinityMask - (ulong.MaxValue << 4), Is.EqualTo(expectedAffinity));
            Assert.That(_affinityView.ShowSmtCores, Is.True);
        }

        [Test]
        public void SelectSmtCores_ShouldReturnNull_WhenMixedCoresAreSelected()
        {
            _affinityView.UpdateAffinityMask(0b1011ul);
            Assert.That(_affinityView.SelectSmtCores, Is.Null);
        }
        [Test]
        public void ShowSmtCores_ShouldReturnCorrectValue()
        {
            // In DEBUG mode, ShowSmtCores should always return true
#if !DEBUG
            // In non-DEBUG mode, ShowSmtCores should return true if there are SMT related cores

            List<CoreInfo> coreInfos = Enumerable.Range(0, 4).Select(i =>
            {
                CoreInfo coreInfo = new() { Id = (uint)i };
                PhysicalCoreGroup physicalCoreGroup = new() { Id = (uint)i };
                coreInfo.AddAssociatedGroup(physicalCoreGroup);
                return coreInfo;
            }).ToList();

            _affinityView = new AffinityView(uint.MaxValue, coreInfos);
            Assert.That(_affinityView.ShowSmtCores, Is.False);
#else
            Assert.Ignore("Only run in release mode.");
#endif
        }

        [Test]
        public void CacheGroupView_ShouldInitializeCorrectly_BasedOnConstructorArguments()
        {
            CoreInfo[] coreInfos = CreateCoreInfosForGroupViewTest();

            AffinityView affinityView = new(0b1010, coreInfos);
            Affinity_manager.ViewWrappers.Affinity.GroupsView<CacheCoreGroup> cacheGroupView = affinityView.CacheGroupView;

            Assert.That(cacheGroupView, Is.Not.Null);
            Assert.That(cacheGroupView.Groups.Select(view => view.CoreGroup), Is.EqualTo(new[] { new CacheCoreGroup { Id = 0, Level = 3 }, new CacheCoreGroup { Id = 1, Level = 3 } }).AsCollection);
        }

        [Test]
        public void PerformanceGroupView_ShouldInitializeCorrectly_BasedOnConstructorArguments()
        {
            CoreInfo[] coreInfos = CreateCoreInfosForGroupViewTest();

            AffinityView affinityView = new(0b1010, coreInfos);
            Affinity_manager.ViewWrappers.Affinity.GroupsView<PerformanceCoreGroup> performanceGroupView = affinityView.PerformanceGroupView;

            Assert.That(performanceGroupView, Is.Not.Null);
            Assert.That(performanceGroupView.Groups.Select(view => view.CoreGroup), Is.EqualTo(new[] { new PerformanceCoreGroup { Id = 3 }, new PerformanceCoreGroup { Id = 1 }, new PerformanceCoreGroup { Id = 0 } }).AsCollection);
        }

        [Test]
        public void CacheGroupView_ShouldBeInSyncWithCoreViews()
        {
            CoreInfo[] coreInfos = CreateCoreInfosForGroupViewTest();
            AffinityView affinityView = new(0b1010, coreInfos);
            Affinity_manager.ViewWrappers.Affinity.GroupsView<CacheCoreGroup> cacheGroupView = affinityView.CacheGroupView;

            // Change the selection state of a cache group
            Assert.That(cacheGroupView.Groups[0].Selected, Is.Not.True);
            cacheGroupView.Groups[0].Selected = true;

            // Verify that the corresponding CoreViews are updated
            foreach (CoreView? coreView in affinityView.LogicalCpus.Where(cpu => cpu.CoreInfo.AssociatedGroups.Contains(cacheGroupView.Groups[0].CoreGroup)))
            {
                Assert.That(coreView.Selected, Is.True);
            }
        }

        [Test]
        public void PerformanceGroupView_ShouldBeInSyncWithCoreViews()
        {
            CoreInfo[] coreInfos = CreateCoreInfosForGroupViewTest();
            AffinityView affinityView = new(0b1010, coreInfos);
            Affinity_manager.ViewWrappers.Affinity.GroupsView<PerformanceCoreGroup> performanceGroupView = affinityView.PerformanceGroupView;

            // Change the selection state of a performance group
            Assert.That(performanceGroupView.Groups[2].Selected, Is.Not.True);
            performanceGroupView.Groups[2].Selected = true;

            // Verify that the corresponding CoreViews are updated
            foreach (CoreView? coreView in affinityView.LogicalCpus.Where(cpu => cpu.CoreInfo.AssociatedGroups.Contains(performanceGroupView.Groups[2].CoreGroup)))
            {
                Assert.That(coreView.Selected, Is.True);
            }
        }

        [Test]
        public void CoreViews_ShouldBeInSyncWithCacheGroupView()
        {
            CoreInfo[] coreInfos = CreateCoreInfosForGroupViewTest();
            AffinityView affinityView = new(0b1010, coreInfos);
            Affinity_manager.ViewWrappers.Affinity.GroupsView<CacheCoreGroup> cacheGroupView = affinityView.CacheGroupView;

            // Change the selection state of a CoreView
            Assert.That(cacheGroupView.Groups[0].Selected, Is.False);
            CoreView coreViewToChange = affinityView.LogicalCpus.First(cpu => cpu.CoreInfo.AssociatedGroups.Contains(cacheGroupView.Groups[0].CoreGroup));
            coreViewToChange.Selected = true;

            // Verify that the corresponding cache group is updated
            Assert.That(cacheGroupView.Groups[0].Selected, Is.True);
        }

        [Test]
        public void CoreViews_ShouldBeInSyncWithPerformanceGroupView()
        {
            CoreInfo[] coreInfos = CreateCoreInfosForGroupViewTest();
            AffinityView affinityView = new(0b1010, coreInfos);
            Affinity_manager.ViewWrappers.Affinity.GroupsView<PerformanceCoreGroup> performanceGroupView = affinityView.PerformanceGroupView;

            // Change the selection state of a CoreView
            Assert.That(performanceGroupView.Groups[2].Selected, Is.False);
            IEnumerable<CoreView> coreViewsToChange = affinityView.LogicalCpus.Where(cpu => cpu.CoreInfo.AssociatedGroups.Contains(performanceGroupView.Groups[2].CoreGroup));
            foreach (CoreView? item in coreViewsToChange)
            {
                item.Selected = true;
            }

            // Verify that the corresponding performance group is updated
            Assert.That(performanceGroupView.Groups[2].Selected, Is.True);
        }


        private static CoreInfo[] CreateCoreInfosForGroupViewTest()
        {
            CoreInfo[] coreInfos = Enumerable.Range(0, 4).Select(id => new CoreInfo { Id = (uint)id }).ToArray();
            coreInfos[0].AddAssociatedGroup(new CacheCoreGroup { Id = 0, Level = 3 });
            coreInfos[1].AddAssociatedGroup(new CacheCoreGroup { Id = 1, Level = 3 });
            coreInfos[2].AddAssociatedGroup(new CacheCoreGroup { Id = 2, Level = 2 });
            coreInfos[3].AddAssociatedGroup(new CacheCoreGroup { Id = 1, Level = 3 });

            coreInfos[0].AddAssociatedGroup(new PerformanceCoreGroup { Id = 0 });
            coreInfos[1].AddAssociatedGroup(new PerformanceCoreGroup { Id = 1 });
            coreInfos[2].AddAssociatedGroup(new PerformanceCoreGroup { Id = 0 });
            coreInfos[3].AddAssociatedGroup(new PerformanceCoreGroup { Id = 3 });

            return coreInfos;
        }
    }
}
