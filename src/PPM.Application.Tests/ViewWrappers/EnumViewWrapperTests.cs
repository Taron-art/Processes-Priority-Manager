using System.Collections.Generic;
using Affinity_manager.Model;
using Affinity_manager.ViewWrappers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace PPM.Application.Tests.ViewWrappers
{
    public enum TestEnum
    {
        Value1,
        Value2
    }

    [TestFixture]
    public class EnumViewWrapperTests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [SetUICulture("en-US")]
        public void DisplayName_ShouldReturnCorrectDisplayName()
        {
            EnumViewWrapper<IoPriority> wrapper = new(IoPriority.Normal);

            string displayName = wrapper.DisplayName;

            Assert.That(displayName, Is.EqualTo("Normal"));
        }

        [Test]
        public void DisplayName_ShouldThrowKeyNotFoundException_WhenResourceNotFound()
        {
            EnumViewWrapper<TestEnum> wrapper = new(TestEnum.Value1);

            // Act & Assert
            ClassicAssert.Throws<KeyNotFoundException>(() => { string displayName = wrapper.DisplayName; });
        }

        [Test]
        public void Equals_ShouldReturnTrue_WhenValuesAreEqual()
        {
            EnumViewWrapper<TestEnum> wrapper1 = new(TestEnum.Value1);
            EnumViewWrapper<TestEnum> wrapper2 = new(TestEnum.Value1);

            bool result = wrapper1.Equals(wrapper2);

            Assert.That(result);
        }

        [Test]
        public void Equals_ShouldReturnFalse_WhenValuesAreNotEqual()
        {
            EnumViewWrapper<TestEnum> wrapper1 = new(TestEnum.Value1);
            EnumViewWrapper<TestEnum> wrapper2 = new(TestEnum.Value2);

            bool result = wrapper1.Equals(wrapper2);

            Assert.That(result, Is.False);
        }

        [Test]
        public void GetHashCode_ShouldReturnSameHashCode_ForEqualValues()
        {
            EnumViewWrapper<TestEnum> wrapper1 = new(TestEnum.Value1);
            EnumViewWrapper<TestEnum> wrapper2 = new(TestEnum.Value1);

            int hashCode1 = wrapper1.GetHashCode();
            int hashCode2 = wrapper2.GetHashCode();

            Assert.That(hashCode2, Is.EqualTo(hashCode1));
        }

        [Test]
        [SetUICulture("en-US")]
        public void ToString_ShouldReturnEnumValueAsString()
        {
            EnumViewWrapper<CpuPriorityClass> wrapper = new(CpuPriorityClass.Low);

            string result = wrapper.ToString();

            Assert.That(result, Is.EqualTo("Low"));
        }
    }
}
