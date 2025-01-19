using System.ComponentModel;
using Affinity_manager.Model.DataGathering;
using Affinity_manager.ViewWrappers;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Events;
using NUnit.Framework;

namespace PPM.Application.Tests.ViewWrappers
{
    [TestFixture]
    public class ProcessInfoViewTests
    {
        private IApplicationIconsLoader _iconLoader;
        private ProcessInfo _processInfo;
        private ProcessInfoView _processInfoView;

        [SetUp]
        public void SetUp()
        {
            _iconLoader = A.Fake<IApplicationIconsLoader>();
            _processInfo = A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor(["TestProcess", Source.RunningTasks]).CallsBaseMethods());
            _processInfoView = new ProcessInfoView(_processInfo, _iconLoader);
        }

        [TearDown]
        public void TearDown()
        {
            _processInfoView.Dispose();
        }

        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            Assert.That(_processInfoView.ProcessInfo, Is.SameAs(_processInfo));
            Assert.That(_processInfoView.IconLoader, Is.SameAs(_iconLoader));
        }

        [Test]
        public void MainModuleName_ShouldReturnProcessInfoMainModuleName()
        {
            Assert.That(_processInfoView.MainModuleName, Is.EqualTo(_processInfo.MainModuleName));
        }

        [Test]
        public void FriendlyName_ShouldReturnProcessInfoFriendlyName()
        {
            A.CallTo(() => _processInfo.FriendlyName).Returns("FriendlyName");
            Assert.That(_processInfoView.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [Test]
        public void PropertyChanged_ShouldBeRaised_WhenProcessFriendlyNameChanges()
        {
            using IMonitor<ProcessInfoView> monitor = _processInfoView.Monitor();
            _processInfo.UpdateWithFriendlyNameAndModulePath("Test1", "Test2");

            monitor.Should().RaisePropertyChangeFor((info) => info.FriendlyName);
            monitor.Should().RaisePropertyChangeFor((info) => info.ApplicationIcon);
        }

        [Test]
        public void Dispose_ShouldUnsubscribeFromProcessInfoPropertyChanged()
        {
            using IMonitor<ProcessInfoView> monitor = _processInfoView.Monitor();
            _processInfoView.Dispose();
            _processInfo.UpdateWithFriendlyNameAndModulePath("Test1", "Test2");
            monitor.Should().NotRaise(nameof(INotifyPropertyChanged.PropertyChanged));
        }
    }
}
