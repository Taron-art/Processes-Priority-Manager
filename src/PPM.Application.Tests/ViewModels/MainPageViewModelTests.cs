using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Affinity_manager.Exceptions;
using Affinity_manager.Model;
using Affinity_manager.Model.CRUD;
using Affinity_manager.ViewModels;
using Affinity_manager.ViewWrappers;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace PPM.Application.Tests.ViewModels
{
    [TestFixture]
    public class MainPageViewModelTests
    {
        private IProcessConfigurationsRepository _repository;
        private IProcessConfigurationViewFactory _viewFactory;
        private MainPageViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _repository = A.Fake<IProcessConfigurationsRepository>();
            _viewFactory = A.Fake<IProcessConfigurationViewFactory>();
            _viewModel = new MainPageViewModel(_repository, _viewFactory);
        }

        [Test]
        public void Add_ShouldAddNewProcessConfigurationView_WhenProcessNameIsValid()
        {
            // Arrange
            _viewModel.NewProcessName = "TestProcess.exe";
            ProcessConfiguration processConfiguration = new("TestProcess");
            ProcessConfigurationView processConfigurationView = new(processConfiguration, A.Fake<IOptionsProvider>());
            A.CallTo(() => _viewFactory.Create(A<ProcessConfiguration>.Ignored)).Returns(processConfigurationView);

            // Act
            _viewModel.Add();

            // Assert
            Assert.That(_viewModel.ProcessesConfigurations, Has.Exactly(1).EqualTo(processConfigurationView));
            Assert.That(_viewModel.SelectedView, Is.EqualTo(processConfigurationView));
            Assert.That(_viewModel.NewProcessName, Is.Empty);
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
        }

        [Test]
        public void Add_ShouldNotAddNewProcessConfigurationView_WhenProcessNameIsInvalud()
        {
            // Arrange
            _viewModel.NewProcessName = "testexe";

            using var monitor = _viewModel.Monitor();

            // Act
            _viewModel.Add();

            // Assert
            Assert.That(_viewModel.ProcessesConfigurations, Is.Empty);

            monitor.Should().Raise(nameof(_viewModel.ShowMessage));
        }

        [Test]
        public async Task SaveChangesAsync_ShouldSaveDirtyProcessConfigurations()
        {
            // Arrange
            IOptionsProvider optionsProviderMock = A.Fake<IOptionsProvider>();

            ProcessConfiguration processConfiguration = new("TestProcess");
            ProcessConfigurationView processConfigurationView = new(processConfiguration, optionsProviderMock);

            ProcessConfiguration processConfiguration1 = new("TestProcess2");
            ProcessConfigurationView processConfigurationView1 = new(processConfiguration1, optionsProviderMock);

            processConfigurationView.AffinityView.AffinityMask = 1U;
            BindingCollectionWithUniqunessCheck<ProcessConfigurationView> processAffinities = new() { processConfigurationView, processConfigurationView1 };
            A.CallTo(() => _viewFactory.CreateCollection(A<IEnumerable<ProcessConfiguration>>.Ignored)).Returns(processAffinities);
            _viewModel = new MainPageViewModel(_repository, _viewFactory);
            await _viewModel.ReloadAsync();

            using FluentAssertions.Events.IMonitor<MainPageViewModel> monitor = _viewModel.Monitor();

            // Act
            await _viewModel.SaveChangesAsync();

            // Assert
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.That.IsSameSequenceAs(new[] { processConfiguration }), A<Func<Task>>.That.IsNotNull())).MustHaveHappened()
                .Then(A.CallTo(() => _repository.Get()).MustHaveHappened());
            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.ProcessesConfigurations);
            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.SaveCancelAvailable);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldRefreshTheListAfterSave()
        {
            // Arrange
            ProcessConfiguration processConfiguration = new("TestProcess");
            ProcessConfigurationView processConfigurationView = new(processConfiguration, A.Fake<IOptionsProvider>());

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

            _viewModel = new MainPageViewModel(_repository, _viewFactory);
            await _viewModel.ReloadAsync();

            using FluentAssertions.Events.IMonitor<MainPageViewModel> monitor = _viewModel.Monitor();

            // Act
            await _viewModel.SaveChangesAsync();

            // Assert
            A.CallTo(() => _repository.SaveAndRestartServiceAsync(A<IEnumerable<ProcessConfiguration>>.Ignored, A<Func<Task>>.Ignored)).MustHaveHappened()
                .Then(A.CallTo(() => _repository.Get()).MustHaveHappened());
            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.ProcessesConfigurations);
            monitor.Should().RaisePropertyChangeFor((viewModel) => viewModel.SaveCancelAvailable);
            Assert.That(_viewModel.ProcessesConfigurations, Is.SameAs(processViewsAfterSave));
        }

        [Test]
        public async Task ReloadAsync_ShouldReloadProcessConfigurations()
        {
            // Arrange
            ProcessConfiguration processConfiguration = new("TestProcess");
            ProcessConfigurationView processConfigurationView = new(processConfiguration, A.Fake<IOptionsProvider>());
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
    }
}
