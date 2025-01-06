using Affinity_manager.Model;
using Affinity_manager.ViewWrappers;
using NUnit.Framework;

namespace PPM.Application.Tests.ViewWrappers
{
    [TestFixture]
    public class ProcessConfigurationEqualityComparerTests
    {
        private ProcessConfigurationViewEqualityComparer _comparer;

        [SetUp]
        public void SetUp()
        {
            _comparer = new ProcessConfigurationViewEqualityComparer();
        }

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            ProcessConfigurationView configView = new(new ProcessConfiguration("Test"), new OptionsProvider());
            Assert.That(_comparer.Equals(configView, configView));
        }

        [Test]
        public void Equals_BothNull_ReturnsTrue()
        {
            Assert.That(_comparer.Equals(null, null));
        }

        [Test]
        public void Equals_OneNull_ReturnsFalse()
        {
            ProcessConfigurationView configView = new(new ProcessConfiguration("Test"), new OptionsProvider());
            Assert.That(_comparer.Equals(configView, null), Is.False);
            Assert.That(_comparer.Equals(null, configView), Is.False);
        }

        [Test]
        public void Equals_DifferentNames_ReturnsFalse()
        {
            ProcessConfigurationView configView1 = new(new ProcessConfiguration("Test1"), new OptionsProvider());
            ProcessConfigurationView configView2 = new(new ProcessConfiguration("Test2"), new OptionsProvider());
            Assert.That(_comparer.Equals(configView1, configView2), Is.False);
        }

        [Test]
        public void Equals_SameNamesDifferentCase_ReturnsTrue()
        {
            ProcessConfigurationView configView1 = new(new ProcessConfiguration("Test"), new OptionsProvider());
            ProcessConfigurationView configView2 = new(new ProcessConfiguration("test"), new OptionsProvider());
            Assert.That(_comparer.Equals(configView1, configView2));
        }

        [Test]
        public void GetHashCode_SameNameDifferentCase_ReturnsSameHashCode()
        {
            ProcessConfigurationView configView1 = new(new ProcessConfiguration("Test"), new OptionsProvider());
            ProcessConfigurationView configView2 = new(new ProcessConfiguration("test"), new OptionsProvider());
            Assert.That(_comparer.GetHashCode(configView2), Is.EqualTo(_comparer.GetHashCode(configView1)));
        }

        [Test]
        public void GetHashCode_DifferentNames_ReturnsDifferentHashCodes()
        {
            ProcessConfigurationView configView1 = new(new ProcessConfiguration("Test1"), new OptionsProvider());
            ProcessConfigurationView configView2 = new(new ProcessConfiguration("Test2"), new OptionsProvider());
            Assert.That(_comparer.GetHashCode(configView2), Is.Not.EqualTo(_comparer.GetHashCode(configView1)));
        }
    }
}