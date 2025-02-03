using Affinity_manager.ViewWrappers;
using Affinity_manager.ViewWrappers.Affinity;
using FluentAssertions;
using FluentAssertions.Events;
using NUnit.Framework;
using PPM.Unsafe;

namespace PPM.Application.Tests.ViewWrappers.Affinity
{
    [TestFixture]
    public class GroupsViewTests
    {
        private GroupsView<CacheCoreGroup> _cacheGroupsView;
        private GroupsView<PerformanceCoreGroup> _performanceGroupsView;
        private CoreGroupView<CacheCoreGroup> _cacheGroupView1;
        private CoreGroupView<CacheCoreGroup> _cacheGroupView2;
        private CoreGroupView<PerformanceCoreGroup> _performanceGroupView1;
        private CoreGroupView<PerformanceCoreGroup> _performanceGroupView2;

        [SetUp]
        public void SetUp()
        {
            _cacheGroupView1 = new TestCacheCoreGroupView() { CoreGroup = new CacheCoreGroup { Id = 1 } };
            _cacheGroupView2 = new TestCacheCoreGroupView() { CoreGroup = new CacheCoreGroup { Id = 2 } };
            _performanceGroupView1 = new TestPerformanceCoreGroupView() { CoreGroup = new PerformanceCoreGroup { Id = 1 } };
            _performanceGroupView2 = new TestPerformanceCoreGroupView() { CoreGroup = new PerformanceCoreGroup { Id = 2 } };

            _cacheGroupsView = new GroupsView<CacheCoreGroup>(_cacheGroupView1, _cacheGroupView2);
            _performanceGroupsView = new GroupsView<PerformanceCoreGroup>(_performanceGroupView1, _performanceGroupView2);
        }

        [Test]
        public void Constructor_ShouldInitializeGroups()
        {
            Assert.That(_cacheGroupsView.Groups.Count, Is.EqualTo(2));
            Assert.That(_performanceGroupsView.Groups.Count, Is.EqualTo(2));
        }

        [Test]
        public void ShouldBeDisplayed_ShouldReturnValue_DependingOnNumberOfGroups()
        {
#if !DEBUG
            var group = new GroupsView<CacheCoreGroup>(_cacheGroupView1);
            Assert.That(group.ShouldBeDisplayed, Is.False);

            group = new GroupsView<CacheCoreGroup>(_cacheGroupView1, _cacheGroupView2);
            Assert.That(group.ShouldBeDisplayed, Is.True);
#else
            Assert.Ignore("Run only in release mode.");
#endif
        }

        [Test]
        public void UpdateGroupsState_ShouldUpdateSelectedState([Values(true, false)] bool secondCoreEnabled)
        {
            CoreInfo coreInfo1 = new() { Id = 1 };
            coreInfo1.AddAssociatedGroup(_cacheGroupView1.CoreGroup);
            coreInfo1.AddAssociatedGroup(_performanceGroupView2.CoreGroup);

            CoreInfo coreInfo2 = new() { Id = 2 };
            coreInfo2.AddAssociatedGroup(_cacheGroupView1.CoreGroup);
            coreInfo2.AddAssociatedGroup(_cacheGroupView2.CoreGroup);

            CoreView[] coreViews = new[]
            {
                new CoreView(false, coreInfo1),
                new CoreView(secondCoreEnabled, coreInfo2)
            };

            _cacheGroupsView.UpdateGroupsState(coreViews);
            if (secondCoreEnabled)
            {
                Assert.That(_cacheGroupView1.Selected, Is.Null);
                Assert.That(_cacheGroupView2.Selected, Is.True);
            }
            else
            {
                Assert.That(_cacheGroupView1.Selected, Is.False);
                Assert.That(_cacheGroupView2.Selected, Is.False);
            }
        }

        [Test]
        public void PropertyChanged_ShouldBeRaised_WhenGroupSelectedChanges()
        {
            using IMonitor<GroupsView<CacheCoreGroup>> monitor = _cacheGroupsView.Monitor();
            _cacheGroupView1.Selected = true;

            monitor.Should()
                .Raise(nameof(_cacheGroupsView.GroupChanged))
                .WithArgs<GroupChangedEventArgs<CacheCoreGroup>>(args => ReferenceEquals(_cacheGroupView1, args.CoreGroupView));
        }

        private class TestCacheCoreGroupView : CoreGroupView<CacheCoreGroup>
        {
            public override string Label => $"Cache Group {CoreGroup.Id}";
        }

        private class TestPerformanceCoreGroupView : CoreGroupView<PerformanceCoreGroup>
        {
            public override string Label => $"Performance Group {CoreGroup.Id}";
        }
    }
}
