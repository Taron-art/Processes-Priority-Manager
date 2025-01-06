using Affinity_manager.ViewWrappers;
using NUnit.Framework;

namespace PPM.Application.Tests.ViewWrappers
{
    [TestFixture]
    public class CoreViewTests
    {
        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            bool expectedValue = true;
            string expectedLabel = "TestLabel";

            // Act
            CoreView coreView = new(expectedValue, expectedLabel);

            // Assert
            Assert.That(coreView.Value, Is.EqualTo(expectedValue));
            Assert.That(coreView.Label, Is.EqualTo(expectedLabel));
        }

        [Test]
        public void CompareTo_ShouldReturnZero_WhenLabelsAreEqual()
        {
            // Arrange
            CoreView coreView1 = new(true, "Label");
            CoreView coreView2 = new(false, "Label");

            // Act
            int result = coreView1.CompareTo(coreView2);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void CompareTo_ShouldReturnPositive_WhenOtherIsNull()
        {
            // Arrange
            CoreView coreView = new(true, "Label");

            // Act
            int result = coreView.CompareTo(null);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void CompareTo_ShouldReturnNegative_WhenLabelIsLessThanOtherLabel()
        {
            // Arrange
            CoreView coreView1 = new(true, "A");
            CoreView coreView2 = new(false, "B");

            // Act
            int result = coreView1.CompareTo(coreView2);

            // Assert
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void CompareTo_ShouldReturnPositive_WhenLabelIsGreaterThanOtherLabel()
        {
            // Arrange
            CoreView coreView1 = new(true, "B");
            CoreView coreView2 = new(false, "A");

            // Act
            int result = coreView1.CompareTo(coreView2);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }
    }
}
