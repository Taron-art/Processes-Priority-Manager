using System;
using System.Linq;
using Affinity_manager.Model;
using Affinity_manager.ViewWrappers;
using NUnit.Framework;
using PPM.Unsafe;

namespace PPM.Application.Tests.ViewWrappers
{
    [TestFixture]
    public class OptionsProviderTests
    {
        private OptionsProvider _optionsProvider;

        [OneTimeSetUp]
        public void OnSetUp()
        {
            _optionsProvider = new OptionsProvider();
        }

        [Test]
        [SetUICulture("en-US")]
        public void CpuPriorities_CorrectList()
        {
            Assert.That(_optionsProvider.CpuPriorities, Has.Count.EqualTo(5));
            Assert.That(_optionsProvider.CpuPriorities.Select(wrapper => wrapper.Value), Is.EqualTo(new CpuPriorityClass[] { CpuPriorityClass.Low, CpuPriorityClass.BelowNormal, CpuPriorityClass.Normal, CpuPriorityClass.AboveNormal, CpuPriorityClass.High }).AsCollection);
        }

        [Test]
        [SetUICulture("en-US")]
        public void IOPriorities_CorrectList()
        {
            Assert.That(_optionsProvider.IoPriorities, Has.Count.EqualTo(3));
            Assert.That(_optionsProvider.IoPriorities.Select(wrapper => wrapper.Value), Is.EqualTo(new IoPriority[] { IoPriority.VeryLow, IoPriority.Low, IoPriority.Normal }).AsCollection);
        }

        [Test]
        [SetUICulture("en-US")]
        public void MemoryPriorities_CorrectList()
        {
            Assert.That(_optionsProvider.MemoryPriorities, Has.Count.EqualTo(5));
            Assert.That(_optionsProvider.MemoryPriorities.Select(wrapper => wrapper.Value), Is.EqualTo(new PagePriority[] { PagePriority.VeryLow, PagePriority.Low, PagePriority.Medium, PagePriority.BelowNormal, PagePriority.Normal }).AsCollection);
        }

        [Test]
        public void NumberOfLogicalCpus_CorrectValue()
        {
            Assert.That(_optionsProvider.NumberOfLogicalCpus, Is.EqualTo(Environment.ProcessorCount));
        }

        [Test]
        public void ProcessorCoresInfo_SmokeValueTest()
        {
            Assert.That(_optionsProvider.ProcessorCoresInfo, Has.Count.EqualTo(Environment.ProcessorCount));

            foreach (var coreInfo in _optionsProvider.ProcessorCoresInfo)
            {
                Assert.That(coreInfo.AssociatedGroups, Is.Not.Empty);
                Assert.That(coreInfo.AssociatedGroups.OfType<PhysicalCoreGroup>, Is.Not.Empty);
                Assert.That(coreInfo.AssociatedGroups.OfType<CacheCoreGroup>, Is.Not.Empty);
                Assert.That(coreInfo.AssociatedGroups.OfType<PerformanceCoreGroup>, Is.Not.Empty);
                Assert.Throws<InvalidOperationException>(() => coreInfo.AddAssociatedGroup(new PhysicalCoreGroup()));
            }
        }
    }
}
