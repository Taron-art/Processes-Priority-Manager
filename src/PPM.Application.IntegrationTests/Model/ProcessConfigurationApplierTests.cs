using System;
using System.Diagnostics;
using System.Linq;
using Affinity_manager.Model;
using NUnit.Framework;
using PPM.Unsafe;

namespace PPM.Application.IntegrationTests.Model
{
    [TestFixture]
    public class ProcessConfigurationApplierTests
    {
        private Process? _testAppProcess;

        [SetUp]
        public void Setup()
        {
            Process.GetProcessesByName("PPM.TestApp").ToList().ForEach(p => p.Kill());

            _testAppProcess = Process.Start(new ProcessStartInfo { FileName = "PPM.TestApp.exe", UseShellExecute = true });
            _testAppProcess!.ProcessorAffinity = 1;
        }

        [TearDown]
        public void TearDown()
        {
            _testAppProcess?.Kill();
            _testAppProcess?.Dispose();
        }

        [Test]
        public void ApplyIfPresent_AppliesConfigurationOnOneProcess()
        {
            ProcessConfiguration configuration = new("PPM.TestApp.exe")
            {
                CpuPriority = CpuPriorityClass.Low,
                CpuAffinityMask = 0xFul << 63 | 2,
                IoPriority = IoPriority.Low,
                MemoryPriority = PagePriority.Medium
            };

            ProcessConfigurationApplier applier = new();
            applier.ApplyIfPresent((byte)Environment.ProcessorCount, configuration);

            using Process? process = Process.GetProcessesByName("PPM.TestApp").FirstOrDefault();
            Assert.That(process, Is.Not.Null);

            Assert.That(process.PriorityClass, Is.EqualTo(ProcessPriorityClass.Idle));
            Assert.That(process.ProcessorAffinity, Is.EqualTo((IntPtr)2));
            Assert.That(process.GetIoPriority(), Is.EqualTo(IoPriorityHint.Low));
            Assert.That(process.GetMemoryPriority(), Is.EqualTo(PagePriorityInformation.Medium));
        }
    }
}
