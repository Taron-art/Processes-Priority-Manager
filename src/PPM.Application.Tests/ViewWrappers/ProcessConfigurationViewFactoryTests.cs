using System.Collections.Generic;
using System.Linq;
using Affinity_manager.Model;
using Affinity_manager.ViewWrappers;
using FakeItEasy;
using NUnit.Framework;

namespace PPM.Application.Tests.ViewWrappers
{
    [TestFixture]
    public class ProcessConfigurationViewFactoryTests
    {
        private IOptionsProvider _optionsProvider;
        private IEqualityComparer<ProcessConfigurationView> _comparer;
        private IProcessConfigurationApplier _configurationApplier;
        private ProcessConfigurationViewFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _optionsProvider = A.Fake<IOptionsProvider>();
            _comparer = A.Fake<IEqualityComparer<ProcessConfigurationView>>();
            _configurationApplier = A.Fake<IProcessConfigurationApplier>();
            _factory = new ProcessConfigurationViewFactory(_optionsProvider, _comparer, _configurationApplier);
        }

        [Test]
        public void Create_ShouldReturnProcessConfigurationView()
        {
            // Arrange
            ProcessConfiguration configuration = new("Name");

            // Act
            ProcessConfigurationView result = _factory.Create(configuration);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<ProcessConfigurationView>());
            Assert.That(result.ProcessConfiguration, Is.SameAs(configuration));
            Assert.That(result.OptionsProvider, Is.SameAs(_optionsProvider));
            Assert.That(result.ConfigurationApplier, Is.SameAs(_configurationApplier));
        }

        [Test]
        public void CreateCollection_ShouldReturnBindingCollectionWithUniqunessCheck()
        {
            // Arrange
            List<ProcessConfiguration> configurations = new()
            {
                new ProcessConfiguration("Test1"),
                new ProcessConfiguration("Test2")
            };

            // Act
            BindingCollectionWithUniqunessCheck<ProcessConfigurationView> result = _factory.CreateCollection(configurations);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<BindingCollectionWithUniqunessCheck<ProcessConfigurationView>>());
            Assert.That(result.Select(view => view.ProcessConfiguration), Is.EqualTo(configurations).AsCollection);
            Assert.That(result.EqualityComparer, Is.SameAs(_comparer));
        }
    }
}
