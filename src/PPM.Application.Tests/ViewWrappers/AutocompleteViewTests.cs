using System.Collections.Generic;
using System.Linq;
using Affinity_manager.Model.DataGathering;
using Affinity_manager.ViewWrappers;
using FakeItEasy;
using NUnit.Framework;

namespace PPM.Application.Tests.ViewWrappers
{
    [TestFixture]
    public class AutocompleteViewTests
    {
        private AutocompleteView _view;
        private IApplicationIconsLoader _applicationIconsLoader;
        private IProcessProvider _processProvider;

        [SetUp]
        public void SetUp()
        {
            _applicationIconsLoader = A.Fake<IApplicationIconsLoader>();
            _processProvider = A.Fake<IProcessProvider>();
            _view = new AutocompleteView(_applicationIconsLoader);
            _view.AddProcessProvider(_processProvider);
        }

        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            Assert.That(_view.ApplicationIconsLoader, Is.SameAs(_applicationIconsLoader));
            Assert.That(_view.AutocompleteProviders, Contains.Item(_processProvider));
        }

        [Test]
        public void GetAutocompleteList_ShouldReturnEmptyArray_WhenSearchStringIsNullOrWhiteSpace()
        {
            _view.AddProcesses(["123"]);
            ProcessInfoView[] result = _view.GetAutocompleteList(null);
            Assert.That(result, Is.Empty);

            result = _view.GetAutocompleteList(string.Empty);
            Assert.That(result, Is.Empty);

            result = _view.GetAutocompleteList(" ");
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAutocompleteList_ShouldReturnOrderedMatchedProcessesFromAllProcessors()
        {
            ProcessInfo processInfo1 = A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor(["Test1", Source.RunningTasks]));

            ProcessInfo processInfo2 = A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor(["Test2", Source.RunningTasks]));

            A.CallTo(() => processInfo1.CompareTo(processInfo2)).Returns(-1);
            A.CallTo(() => processInfo2.CompareTo(processInfo1)).Returns(1);

            IProcessProvider secondProvider = A.Fake<IProcessProvider>();
            A.CallTo(() => secondProvider.GetMatchedProcesses("test")).Returns(new List<ProcessInfo>() { processInfo1 });
            A.CallTo(() => _processProvider.GetMatchedProcesses("test")).Returns(new List<ProcessInfo> { processInfo2 });

            _view.AddProcessProvider(secondProvider);
            ProcessInfoView[] result = _view.GetAutocompleteList("test");

            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result[0].ProcessInfo, Is.SameAs(processInfo1));
            Assert.That(result[1].ProcessInfo, Is.SameAs(processInfo2));
        }

        [Test]
        public void GetAutocompleteList_ShouldReturnAHigherRatedCopyOfToIdenticalProcesses()
        {
            ProcessInfo processInfo1 = A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor(["Test1", Source.ExistingProfiles]).CallsBaseMethods());
            A.CallTo(() => processInfo1.Rating).Returns<byte>(0);

            ProcessInfo processInfo2 = A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor(["Test1", Source.RunningTasks]).CallsBaseMethods());
            A.CallTo(() => processInfo2.Rating).Returns<byte>(2);

            ProcessInfo processInfo3 = A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor(["Test1", Source.None]).CallsBaseMethods());
            A.CallTo(() => processInfo3.Rating).Returns<byte>(1);

            foreach (ProcessInfo first in new ProcessInfo[] { processInfo1, processInfo2, processInfo3 })
            {
                foreach (ProcessInfo second in new ProcessInfo[] { processInfo1, processInfo2, processInfo3 })
                {
                    A.CallTo(() => first.CompareTo(second)).Returns(0);
                }
            }

            IProcessProvider secondProvider = A.Fake<IProcessProvider>();
            A.CallTo(() => _processProvider.GetMatchedProcesses("test")).Returns(new List<ProcessInfo> { processInfo1, processInfo3 });
            A.CallTo(() => secondProvider.GetMatchedProcesses("test")).Returns(new List<ProcessInfo> { processInfo2, processInfo3 });

            _view.AddProcessProvider(secondProvider);
            ProcessInfoView[] result = _view.GetAutocompleteList("test");

            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].ProcessInfo, Is.SameAs(processInfo2));
        }

        [Test]
        public void ClearCache_ShouldClearAllCachedProcessInfoViews()
        {
            ProcessInfo processInfo = A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor(["test", Source.None]));
            A.CallTo(() => _processProvider.GetMatchedProcesses("test")).Returns(new List<ProcessInfo> { processInfo });

            ProcessInfoView[] views = _view.GetAutocompleteList("test");
            ProcessInfoView[] views2 = _view.GetAutocompleteList("test");
            Assert.That(views[0], Is.SameAs(views2[0]));
            _view.ClearCache();
            ProcessInfoView[] views3 = _view.GetAutocompleteList("test");
            Assert.That(views[0], Is.Not.SameAs(views3[0]));
        }

        [Test]
        public void AddProcesses_ShouldAddProcessesToManualAutocompleteProvider()
        {
            List<string> processes = ["process1", "process2"];
            _view.AddProcesses(processes);

            ManualAutocompleteProvider? manualProvider = _view.AutocompleteProviders.OfType<ManualAutocompleteProvider>().FirstOrDefault();
            Assert.That(manualProvider, Is.Not.Null);
            Assert.That(manualProvider.GetMatchedProcesses("process1").First().MainModuleName, Is.EqualTo(processes[0]));
        }
    }
}
