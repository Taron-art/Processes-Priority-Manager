using System.Threading.Tasks;
using Affinity_manager.Utils;
using Affinity_manager.ViewWrappers.Affinity;
using NUnit.Framework;
using PPM.Unsafe;
using VerifyNUnit;

namespace PPM.Application.Tests.ViewWrappers.Affinity
{
    [TestFixture]
    public class CoreViewTests
    {
        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            bool expectedValue = true;
            uint coreId = 1;
            CoreInfo coreInfo = new() { Id = coreId };
            string expectedLabel = string.Format(Affinity_manager.Strings.PPM.CpuFormat, coreId);

            // Act
            CoreView coreView = new(expectedValue, coreInfo);

            // Assert
            Assert.That(coreView.Selected, Is.EqualTo(expectedValue));
            Assert.That(coreView.Label, Is.EqualTo(expectedLabel));
            Assert.That(coreView.CoreInfo, Is.EqualTo(coreInfo));
        }

        [Test]
        public void CompareTo_ShouldReturnZero_WhenLabelsAreEqual()
        {
            // Arrange
            CoreInfo coreInfo1 = new() { Id = 1 };
            CoreInfo coreInfo2 = new() { Id = 1 };
            CoreView coreView1 = new(true, coreInfo1);
            CoreView coreView2 = new(false, coreInfo2);

            // Act
            int result = coreView1.CompareTo(coreView2);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void CompareTo_ShouldReturnPositive_WhenOtherIsNull()
        {
            // Arrange
            CoreInfo coreInfo = new() { Id = 1 };
            CoreView coreView = new(true, coreInfo);

            // Act
            int result = coreView.CompareTo(null);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void CompareTo_ShouldReturnNegative_WhenLabelIsLessThanOtherLabel()
        {
            // Arrange
            CoreInfo coreInfo1 = new() { Id = 1 };
            CoreInfo coreInfo2 = new() { Id = 2 };
            CoreView coreView1 = new(true, coreInfo1);
            CoreView coreView2 = new(false, coreInfo2);

            // Act
            int result = coreView1.CompareTo(coreView2);

            // Assert
            Assert.That(result, Is.LessThan(0));
        }

        [Test]
        public void CompareTo_ShouldReturnPositive_WhenLabelIsGreaterThanOtherLabel()
        {
            // Arrange
            CoreInfo coreInfo1 = new() { Id = 2 };
            CoreInfo coreInfo2 = new() { Id = 1 };
            CoreView coreView1 = new(true, coreInfo1);
            CoreView coreView2 = new(false, coreInfo2);

            // Act
            int result = coreView1.CompareTo(coreView2);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Description_ShouldIncludePhysicalCoreGroupInfo()
        {
            // Arrange
            CoreInfo coreInfo = new() { Id = 1 };
            coreInfo.AddAssociatedGroup(new PhysicalCoreGroup { Id = 1 });
            coreInfo.Seal();
            CoreView coreView = new(true, coreInfo);

            // Act
            string? description = coreView.Description;

            // Assert
            Assert.That(description, Does.Contain(string.Format(Affinity_manager.Strings.PPM.PhysicalCoreGroupFormat, 1)));
        }

        [Test]
        public void Description_ShouldIncludePerformanceCoreGroupInfo()
        {
            // Arrange
            CoreInfo coreInfo = new() { Id = 1 };
            coreInfo.AddAssociatedGroup(new PerformanceCoreGroup { Id = 1 });
            coreInfo.Seal();
            CoreView coreView = new(true, coreInfo);

            // Act
            string? description = coreView.Description;

            // Assert
            Assert.That(description, Does.Contain(string.Format(Affinity_manager.Strings.PPM.PerformanceClassFormat, 1)));
        }

        [Test]
        public void Description_ShouldIncludeCacheCoreGroupInfo()
        {
            // Arrange
            CoreInfo coreInfo = new() { Id = 1 };
            coreInfo.AddAssociatedGroup(new CacheCoreGroup { Id = 1, Level = 1, CacheSizeInB = 1024 });
            coreInfo.Seal();
            CoreView coreView = new(true, coreInfo);

            // Act
            string? description = coreView.Description;

            // Assert
            Assert.That(description, Does.Contain(string.Format(Affinity_manager.Strings.PPM.CacheLevelFormat, 1)));
            Assert.That(description, Does.Contain(SizeFormatHelper.SizeToString(1024)));
        }

        [Test]
        public void Description_ShouldHandleMultipleCacheCoreGroups()
        {
            // Arrange
            CoreInfo coreInfo = new() { Id = 1 };
            coreInfo.AddAssociatedGroup(new CacheCoreGroup { Id = 1, Level = 1, CacheSizeInB = 1024 });
            coreInfo.AddAssociatedGroup(new CacheCoreGroup { Id = 2, Level = 1, CacheSizeInB = 2048 });
            coreInfo.Seal();
            CoreView coreView = new(true, coreInfo);

            // Act
            string? description = coreView.Description;

            // Assert
            Assert.That(description, Does.Contain(string.Format(Affinity_manager.Strings.PPM.CacheLevelFormat, 1)));
            Assert.That(description, Does.Contain(SizeFormatHelper.SizeToString(1024)));
            Assert.That(description, Does.Contain(string.Format(Affinity_manager.Strings.PPM.CacheAdditionFormat, SizeFormatHelper.SizeToString(2048))));
        }


        [Test]
        [Culture("uk-UA")]
        public Task Description_SnapshotTest()
        {
            CoreInfo coreInfo = new() { Id = 1 };
            coreInfo.AddAssociatedGroup(new PhysicalCoreGroup { Id = 1 });
            coreInfo.AddAssociatedGroup(new PerformanceCoreGroup { Id = 1 });
            coreInfo.AddAssociatedGroup(new CacheCoreGroup { Id = 1, Level = 1, CacheSizeInB = 1024 });
            coreInfo.AddAssociatedGroup(new CacheCoreGroup { Id = 2, Level = 1, CacheSizeInB = 2048 });
            coreInfo.AddAssociatedGroup(new CacheCoreGroup { Id = 3, Level = 3, CacheSizeInB = 5 });
            coreInfo.AddAssociatedGroup(new CacheCoreGroup { Id = 4, Level = 3, CacheSizeInB = 17 });
            coreInfo.Seal();
            CoreView coreView = new(true, coreInfo);

            return Verifier.Verify(coreView.Description);
        }
    }
}
