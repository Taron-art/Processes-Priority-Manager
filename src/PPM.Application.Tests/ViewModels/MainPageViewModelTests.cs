using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Affinity_manager.Exceptions;
using Affinity_manager.Model;
using Affinity_manager.Model.CRUD;
using Affinity_manager.Model.DataGathering;
using Affinity_manager.ViewModels;
using Affinity_manager.ViewWrappers;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Events;
using NUnit.Framework;
using PPM.Unsafe;

namespace PPM.Application.Tests.ViewModels
{
    [TestFixture]
    public class MainPageViewModelTests
    {
        private IProcessConfigurationsRepository _repository;
        private IProcessConfigurationViewFactory _viewFactory;
        private IAutocompleteProvider _autocompleteProvider;
        private IProcessConfigurationApplier _configurationApplier;
        private MainPageViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _repository = A.Fake<IProcessConfigurationsRepository>();
            _viewFactory = A.Fake<IProcessConfigurationViewFactory>();
            _autocompleteProvider = A.Fake<IAutocompleteProvider>();
            _configurationApplier = A.Fake<IProcessConfigurationApplier>();
            _viewModel = new MainPageViewModel(_repository, _viewFactory, _autocompleteProvider, _configurationApplier);
        }

        [Test]
        public void Add_ShouldAddNewProcessConfigurationView_WhenProcessNameIsValid()
        {
            // Arrange
            _viewModel.NewProcessName = "TestProcess.exe";
            ProcessConfiguration processConfiguration = new("TestProcess");
            ProcessConfigurationView processConfigurationView = new(processConfiguration, CreateFakeOptionsProvider(), _configurationApplier);
            A.CallTo(() => _viewFactory.Create(A<ProcessConfiguration>.Ignored)).Returns(processConfigurationView);

            // Act
            _viewModel.Add();

            // Assert
            Assert.That(_viewModel.ProcessesConfigurations, Has.Exactly(1).EqualTo(processConfigurationView));
            Assert.That(_viewModel.SelectedView, Is.EqualTo(processConfigurationView));
            Assert.That(_viewModel.NewProcessName, Is.Empty);
            A.CallTo(() => _autocompleteProvider.AddProcesses(A<IEnumerable<string>>.That.Contains("TestProcess.exe"))).MustHaveHappened();
            A.CallTo(() => _autocompleteProvider.ClearCache()).MustHaveHappened();
        }

        [Test]
        public void Add_ShouldNotAddNewProcessConfigurationView_WhenProcessNameIsEmpty()
        {
            // Arrange
            _viewModel.NewProcessName = " ";

            // Act
            _viewModel.Add();

            // Assert
            Assert.That(_viewModel.ProcessesConfigurations, Is.Empty);
            A.CallTo(() => _autocompleteProvider.AddProcesses(A<IEnumerable<string>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _autocompleteProvider.ClearCache()).MustNotHaveHappened();
        }

        [Test]
        public void Add_ShouldNotAddNewProcessConfigurationView_WhenProcessNameIsInvalid()
        {
            // Arrange
            _viewModel.NewProcessName = "testexe";

            using IMonitor<MainPageViewModel> monitor = _viewModel.Monitor();

            // Act
            _viewModel.Add();

            // Assert
            Assert.That(_viewModel.ProcessesConfigurations, Is.Empty);
            monitor.Should().Raise(nameof(_viewModel.ShowMessage));
            A.CallTo(() => _autocompleteProvider.AddProcesses(A<IEnumerable<string>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _autocompleteProvider.ClearCache()).MustNotHaveHappened();
        }

        [Test]
        public async Task SaveChangesAsync_ShouldSaveDirtyProcessConfigurations()
        {
            // Arrange
            IOptionsProvider optionsProviderMock = CreateFakeOptionsProvider();

            ProcessConfiguration processConfiguration = new("TestProcess");
            ProcessConfigurationView processConfigurationView = new(processConfiguration, optionsProviderMock, _configurationApplier);

            ProcessConfiguration processConfiguration1 = new("TestProcess2");
            ProcessConfigurationView processConfigurationView1 = new(processConfiguration1, optionsProviderMock, _configurationApplier);

            processConfigurationView.AffinityView.AffinityMask = 1U;
            BindingCollectionWithUniqunessCheck<ProcessConfigurationView> processAffinities = new() { processConfigurationView, processConfigurationView1 };
            A.CallTo(() => _viewFactory.CreateCollection(A<IEnumerable<ProcessConfiguration>>.Ignored)).Returns(processAffinities);
            _viewModel = new MainPageViewModel(_repository, _viewFactory, _autocompleteProvider, _configurationApplier);
            await _viewModel.ReloadAsync();

            using IMonitor<MainPageViewModel> monitor = _viewModel.Monitor();

            // Act
            await _viewModel.SaveChangesAsync();

            // Assert
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.That.IsSameSequenceAs(new[] { processConfiguration }), A<Func<Task>>.That.IsNotNull())).MustHaveHappened()
                .Then(A.CallTo(() => _repository.Get()).MustHaveHappened());
            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.ProcessesConfigurations);
            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.IsSaveAvailable);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldRefreshTheListAfterSave()
        {
            // Arrange
            ProcessConfiguration processConfiguration = new("TestProcess");
            ProcessConfigurationView processConfigurationView = new(processConfiguration, CreateFakeOptionsProvider(), _configurationApplier);

            // These are expectations that are returned before Save.
            BindingCollectionWithUniqunessCheck<ProcessConfigurationView> processViews = new() { processConfigurationView };
            List<ProcessConfiguration> repositoryList = new() { processConfiguration };

            // These are expectations that are returned after Save.
            BindingCollectionWithUniqunessCheck<ProcessConfigurationView> processViewsAfterSave = new() { processConfigurationView };
            List<ProcessConfiguration> repositoryListAfterSave = new() { processConfiguration };

            A.CallTo(() => _repository.Get()).Returns(repositoryList);
            A.CallTo(() => _viewFactory.CreateCollection(repositoryList)).Returns(processViews);

            A.CallTo(() => _repository.Get()).Returns(repositoryListAfterSave);
            A.CallTo(() => _viewFactory.CreateCollection(repositoryListAfterSave)).Returns(processViewsAfterSave);

            _viewModel = new MainPageViewModel(_repository, _viewFactory, _autocompleteProvider, _configurationApplier);
            await _viewModel.ReloadAsync();

            using FluentAssertions.Events.IMonitor<MainPageViewModel> monitor = _viewModel.Monitor();

            // Act
            await _viewModel.SaveChangesAsync();

            // Assert
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.Ignored, A<Func<Task>>.Ignored)).MustHaveHappened()
                .Then(A.CallTo(() => _repository.Get()).MustHaveHappened());
            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.ProcessesConfigurations);
            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.IsSaveAvailable);
            Assert.That(_viewModel.ProcessesConfigurations, Is.SameAs(processViewsAfterSave));
        }

        [Test]
        public async Task ReloadAsync_ShouldReloadProcessConfigurations()
        {
            // Arrange
            ProcessConfiguration processConfiguration = new("TestProcess");
            ProcessConfigurationView processConfigurationView = new(processConfiguration, CreateFakeOptionsProvider(), _configurationApplier);
            BindingCollectionWithUniqunessCheck<ProcessConfigurationView> processConfigurations = new() { processConfigurationView };
            A.CallTo(() => _viewFactory.CreateCollection(A<IEnumerable<ProcessConfiguration>>.Ignored)).Returns(processConfigurations);

            // Act
            await _viewModel.ReloadAsync();

            // Assert
            Assert.That(_viewModel.ProcessesConfigurations, Is.SameAs(processConfigurations));
            Assert.That(_viewModel.SelectedView, Is.EqualTo(processConfigurationView));
        }

        [Test]
        [SetUICulture("en-US")]
        public void SaveChangesAsync_ShouldShowMessage_WhenServiceNotInstalledExceptionIsThrown()
        {
            // Arrange
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.Ignored, A<Func<Task>>.Ignored))
                .Throws(new ServiceNotInstalledException("Name"));
            using FluentAssertions.Events.IMonitor<MainPageViewModel> monitor = _viewModel.Monitor();

            // Act & Assert
            Assert.DoesNotThrowAsync(() => _viewModel.SaveChangesAsync());
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.Ignored, A<Func<Task>>.Ignored)).MustHaveHappened();

            monitor.Should().Raise(nameof(_viewModel.ShowMessage))
                .WithArgs<string>((message) => message.Equals(Affinity_manager.Strings.PPM.ServiceNotFountErrorMessage));
        }

        [Test]
        [SetUICulture("en-US")]
        public void SaveChangesAsync_ShouldShowMessage_WhenValidationExceptionIsThrown()
        {
            // Arrange
            string message = TestContext.CurrentContext.Random.GetString();
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.Ignored, A<Func<Task>>.Ignored))
                .Throws(new ValidationException(message));
            using FluentAssertions.Events.IMonitor<MainPageViewModel> monitor = _viewModel.Monitor();

            // Act & Assert
            Assert.DoesNotThrowAsync(() => _viewModel.SaveChangesAsync());
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.Ignored, A<Func<Task>>.Ignored)).MustHaveHappened();

            monitor.Should().Raise(nameof(_viewModel.ShowMessage))
                .WithArgs<string>((message) => message.Equals(message));
        }

        [Test]
        public void SaveChangesAsync_ShouldSetSaveInProgressToFalse_WhenSaveIsCompleted()
        {
            using IMonitor<MainPageViewModel> monitor = _viewModel.Monitor();
            // Arrange
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.Ignored, A<Func<Task>>.Ignored))
                .Invokes(() =>
                {
                    Assert.That(_viewModel.SaveInProgress, Is.True);
                    monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.SaveInProgress);
                    monitor.Clear();
                })
                .Returns(Task.CompletedTask);


            // Act
            Assert.DoesNotThrowAsync(() => _viewModel.SaveChangesAsync());
            // Assert
            Assert.That(_viewModel.SaveInProgress, Is.False);

            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.SaveInProgress);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldApplyConfigurationsAsync([Values(true, false)] bool applyOnRunningProcess)
        {
            // Arrange
            IOptionsProvider optionsProvider = CreateFakeOptionsProvider();

            ProcessConfiguration processConfiguration = new("TestProcess");
            A.CallTo(() => _repository.Get()).Returns(new List<ProcessConfiguration> { processConfiguration });

            ProcessConfigurationView processConfigurationView = new(processConfiguration, optionsProvider, _configurationApplier);

            BindingCollectionWithUniqunessCheck<ProcessConfigurationView> processAffinities = new() { processConfigurationView };
            A.CallTo(() => _viewFactory.CreateCollection(A<IEnumerable<ProcessConfiguration>>.Ignored)).Returns(processAffinities);
            _viewModel = new MainPageViewModel(_repository, _viewFactory, _autocompleteProvider, _configurationApplier);
            _viewModel.ApplyOnRunningProcesses = applyOnRunningProcess;
            await _viewModel.ReloadAsync();
            // Act
            await _viewModel.SaveChangesAsync();
            // Assert
            if (applyOnRunningProcess)
            {
                A.CallTo(() => _configurationApplier.ApplyIfPresent(5, processConfigurationView.ProcessConfiguration)).MustHaveHappened();
            }
            else
            {
                A.CallTo(() => _configurationApplier.ApplyIfPresent(5, processConfigurationView.ProcessConfiguration)).WithAnyArguments().MustNotHaveHappened();
            }
        }

        [Test]
        public void GetAutoCompleteList_ShouldReturnAutocompleteListBasedOnNewProcessName()
        {
            // Arrange
            string processName = "TestProcess";
            _viewModel.NewProcessName = processName;
            ProcessInfoView[] expectedList =
            [
                new ProcessInfoView(A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor([processName, Source.None])), A.Fake<IApplicationIconsLoader>()),
                new ProcessInfoView(A.Fake<ProcessInfo>(x => x.WithArgumentsForConstructor([processName, Source.None])), A.Fake<IApplicationIconsLoader>())
            ];
            A.CallTo(() => _autocompleteProvider.GetAutocompleteList(processName)).Returns(expectedList);

            // Act
            ProcessInfoView[] result = _viewModel.GetAutoCompleteList();

            // Assert
            Assert.That(result, Is.SameAs(expectedList));
            A.CallTo(() => _autocompleteProvider.GetAutocompleteList(processName)).MustHaveHappened();
        }


        private static IOptionsProvider CreateFakeOptionsProvider()
        {
            IOptionsProvider optionsProvider = A.Fake<IOptionsProvider>();
            A.CallTo(() => optionsProvider.NumberOfLogicalCpus).Returns(5u);
            A.CallTo(() => optionsProvider.ProcessorCoresInfo).Returns((IReadOnlyList<CoreInfo>)A.CollectionOfFake<CoreInfo>(5));
            return optionsProvider;
        }
    }
}
