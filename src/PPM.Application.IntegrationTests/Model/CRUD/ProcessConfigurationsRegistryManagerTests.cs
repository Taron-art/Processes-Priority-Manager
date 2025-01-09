using System.Collections.Generic;
using System.IO;
using System.Linq;
using Affinity_manager.Model;
using Affinity_manager.Model.CRUD;
using Microsoft.Win32;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace PPM.Application.IntegrationTests.Model.CRUD
{
    [TestFixture]
    internal class ProcessConfigurationsRegistryManagerTests
    {
        private const string IfeoRegistryKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        private const string AppOptionsRegistryKeyPath = @"SOFTWARE\Processes Priority Manager";
        private const string PerfOptionsSubKey = "PerfOptions";
        private const string CpuPriorityClassValueName = "CpuPriorityClass";
        private const string IoPriorityValueName = "IoPriority";
        private const string PagePriorityValueName = "PagePriority";
        private string _ifeoRegistryBackupPath;
        private string? _appRegistryBackupPath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            using (Registry.LocalMachine.OpenSubKey(IfeoRegistryKeyPath, true)) { };
            _ifeoRegistryBackupPath = Path.Combine(Path.GetTempPath(), "backupIfeo.hiv");
            RegistryTestsHelpers.BackupRegistryKey(IfeoRegistryKeyPath, _ifeoRegistryBackupPath);
            if (Registry.LocalMachine.OpenSubKey(AppOptionsRegistryKeyPath, false) != null)
            {
                _appRegistryBackupPath = Path.Combine(Path.GetTempPath(), "backupApp.hiv");
                RegistryTestsHelpers.BackupRegistryKey(AppOptionsRegistryKeyPath, _appRegistryBackupPath);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            RegistryTestsHelpers.RestoreRegistryKey(IfeoRegistryKeyPath, _ifeoRegistryBackupPath);
            File.Delete(_ifeoRegistryBackupPath);
            if (_appRegistryBackupPath != null)
            {
                RegistryTestsHelpers.RestoreRegistryKey(AppOptionsRegistryKeyPath, _appRegistryBackupPath);
                File.Delete(_appRegistryBackupPath);
            }
            else
            {
                Registry.LocalMachine.DeleteSubKeyTree(AppOptionsRegistryKeyPath, false);
            }
        }

        [Test]
        public void LoadFromRegistry_ReturnsPerformanceOptionsFromIFEO()
        {
            // Arrange
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string testKeyPath = Path.Combine(IfeoRegistryKeyPath, imageName);

            using (RegistryKey testKey = Registry.LocalMachine.CreateSubKey(testKeyPath))
            {
                using RegistryKey perfOptionsKey = testKey.CreateSubKey(PerfOptionsSubKey);
                perfOptionsKey.SetValue(CpuPriorityClassValueName, (int)CpuPriorityClass.High, RegistryValueKind.DWord);
                perfOptionsKey.SetValue(IoPriorityValueName, (int)IoPriority.Low, RegistryValueKind.DWord);
                perfOptionsKey.SetValue(PagePriorityValueName, (int)PagePriority.BelowNormal, RegistryValueKind.DWord);
            }

            try
            {
                List<ProcessConfiguration> configurations = manager.LoadFromRegistry();
                AssertProcessConfigurationPerformance(imageName, configurations, (CpuPriorityClass.High, IoPriority.Low, PagePriority.BelowNormal));
            }
            finally
            {
                Registry.LocalMachine.DeleteSubKeyTree(testKeyPath);
            }
        }

        [Test]
        public void LoadFromRegistry_ReturnsWhenCpuPriorityHasWrongType()
        {
            // Arrange
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string testKeyPath = Path.Combine(IfeoRegistryKeyPath, imageName);

            using (RegistryKey testKey = Registry.LocalMachine.CreateSubKey(testKeyPath))
            {
                using RegistryKey perfOptionsKey = testKey.CreateSubKey(PerfOptionsSubKey);
                perfOptionsKey.SetValue(CpuPriorityClassValueName, "InvalidType", RegistryValueKind.String);
                perfOptionsKey.SetValue(IoPriorityValueName, (int)IoPriority.Low, RegistryValueKind.DWord);
            }

            try
            {
                List<ProcessConfiguration> configurations = manager.LoadFromRegistry();
                AssertProcessConfigurationPerformance(imageName, configurations, (CpuPriorityClass.Normal, IoPriority.Low, PagePriority.Normal));
            }
            finally
            {
                Registry.LocalMachine.DeleteSubKeyTree(testKeyPath);
            }
        }

        [Test]
        public void LoadFromRegistry_ReturnsWhenIoPriorityHasWrongType()
        {
            // Arrange
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string testKeyPath = Path.Combine(IfeoRegistryKeyPath, imageName);

            using (RegistryKey testKey = Registry.LocalMachine.CreateSubKey(testKeyPath))
            {
                using RegistryKey perfOptionsKey = testKey.CreateSubKey(PerfOptionsSubKey);
                perfOptionsKey.SetValue(CpuPriorityClassValueName, (int)CpuPriorityClass.Low, RegistryValueKind.DWord);
                perfOptionsKey.SetValue(IoPriorityValueName, "1", RegistryValueKind.String);
            }

            try
            {
                List<ProcessConfiguration> configurations = manager.LoadFromRegistry();
                AssertProcessConfigurationPerformance(imageName, configurations, (CpuPriorityClass.Low, IoPriority.Normal, PagePriority.Normal));
            }
            finally
            {
                Registry.LocalMachine.DeleteSubKeyTree(testKeyPath);
            }
        }

        [Test]
        public void LoadFromRegistry_ReturnsWhenPagePriorityHasWrongType()
        {
            // Arrange
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string testKeyPath = Path.Combine(IfeoRegistryKeyPath, imageName);

            using (RegistryKey testKey = Registry.LocalMachine.CreateSubKey(testKeyPath))
            {
                using RegistryKey perfOptionsKey = testKey.CreateSubKey(PerfOptionsSubKey);
                perfOptionsKey.SetValue(CpuPriorityClassValueName, (int)CpuPriorityClass.Low, RegistryValueKind.DWord);
                perfOptionsKey.SetValue(PagePriorityValueName, "1", RegistryValueKind.String);
            }

            try
            {
                List<ProcessConfiguration> configurations = manager.LoadFromRegistry();
                AssertProcessConfigurationPerformance(imageName, configurations, (CpuPriorityClass.Low, IoPriority.Normal, PagePriority.Normal));
            }
            finally
            {
                Registry.LocalMachine.DeleteSubKeyTree(testKeyPath);
            }
        }

        [Test]
        public void LoadFromRegistry_ReturnsFromBothLocations()
        {
            // Arrange
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string imageOptionsTestKeyPath = Path.Combine(ImageOptionsFiller.ImageOptionsRegistryPath, imageName);
            string ifeoTestKeyPath = Path.Combine(IfeoRegistryKeyPath, imageName);

            using (RegistryKey testKey = Registry.LocalMachine.CreateSubKey(imageOptionsTestKeyPath))
            {
                testKey.SetValue(ImageOptionsFiller.AffinityValueName, 123L, RegistryValueKind.QWord);
            }

            using (RegistryKey testKey = Registry.LocalMachine.CreateSubKey(ifeoTestKeyPath))
            {
                using RegistryKey perfOptionsKey = testKey.CreateSubKey(PerfOptionsSubKey);
                perfOptionsKey.SetValue(CpuPriorityClassValueName, (int)CpuPriorityClass.Low, RegistryValueKind.DWord);
                perfOptionsKey.SetValue(IoPriorityValueName, (int)IoPriority.VeryLow, RegistryValueKind.DWord);
                perfOptionsKey.SetValue(PagePriorityValueName, (int)PagePriority.Medium, RegistryValueKind.DWord);
            }

            try
            {
                // Act
                List<ProcessConfiguration> configurations = manager.LoadFromRegistry();

                // Assert
                ProcessConfiguration? config = configurations.SingleOrDefault(c => c.Name == imageName);
                Assert.That(config, Is.Not.Null);
                Assert.That(config.CpuAffinityMask, Is.EqualTo(123U));
                Assert.That(config.CpuPriority, Is.EqualTo(CpuPriorityClass.Low));
                Assert.That(config.IoPriority, Is.EqualTo(IoPriority.VeryLow));
                Assert.That(config.MemoryPriority, Is.EqualTo(PagePriority.Medium));
            }
            finally
            {
                // Cleanup
                Registry.LocalMachine.DeleteSubKeyTree(imageOptionsTestKeyPath);
                Registry.LocalMachine.DeleteSubKeyTree(ifeoTestKeyPath);
            }
        }

        [Test]
        public void LoadFromRegistry_DoesNotReturnWhenAdditionalPerfOptionsArePresent()
        {
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string testKeyPath = Path.Combine(IfeoRegistryKeyPath, imageName);
            string testPerfOptionsPath = Path.Combine(testKeyPath, PerfOptionsSubKey);

            using (RegistryKey testKey = Registry.LocalMachine.CreateSubKey(testKeyPath))
            {
                using RegistryKey perfOptionsKey = testKey.CreateSubKey(PerfOptionsSubKey);
                perfOptionsKey.SetValue("SomeKey", 1, RegistryValueKind.DWord);
            }

            try
            {
                List<ProcessConfiguration> configurations = manager.LoadFromRegistry();
                Assert.That(configurations.SingleOrDefault(item => item.Name == imageName), Is.Null);
            }
            finally
            {
                Registry.LocalMachine.DeleteSubKeyTree(testKeyPath);
            }
        }

        [Test]
        public void SaveToRegistry_SavesPerformanceOptionsToIFEO()
        {
            // Arrange
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string testKeyPath = Path.Combine(IfeoRegistryKeyPath, imageName);
            ProcessConfiguration config = new(imageName)
            {
                CpuPriority = CpuPriorityClass.AboveNormal,
                IoPriority = IoPriority.Low,
                MemoryPriority = PagePriority.BelowNormal
            };

            try
            {
                // Act
                manager.SaveToRegistry([config]);

                // Assert
                using (RegistryKey? testKey = Registry.LocalMachine.OpenSubKey(testKeyPath))
                {
                    Assert.That(testKey, Is.Not.Null);
                    using RegistryKey? perfOptionsKey = testKey.OpenSubKey(PerfOptionsSubKey);
                    Assert.That(perfOptionsKey, Is.Not.Null);
                    Assert.That(perfOptionsKey.GetValue(CpuPriorityClassValueName), Is.EqualTo((int)CpuPriorityClass.AboveNormal));
                    Assert.That(perfOptionsKey.GetValue(IoPriorityValueName), Is.EqualTo((int)IoPriority.Low));
                    Assert.That(perfOptionsKey.GetValue(PagePriorityValueName), Is.EqualTo((int)PagePriority.BelowNormal));
                }

                // Now lets try to reset and save it again
                config.Reset();
                config.CpuAffinityMask = 123U; // With this it should still remove IFEO options
                manager.SaveToRegistry([config]);

                using (RegistryKey? testKey = Registry.LocalMachine.OpenSubKey(testKeyPath))
                {
                    Assert.That(testKey, Is.Null);
                }
            }
            finally
            {
                // Cleanup
                Registry.LocalMachine.DeleteSubKeyTree(testKeyPath, false);
            }
        }

        [Test]
        public void SaveToRegistry_RemovesEmptySubKeysInIFEO()
        {
            // Arrange
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string testKeyPath = Path.Combine(IfeoRegistryKeyPath, imageName);
            ProcessConfiguration config = new(imageName)
            {
                CpuAffinityMask = 123U // This should not create any IFEO options
            };

            // Act
            manager.SaveToRegistry([config]);

            // Assert
            using RegistryKey? testKey = Registry.LocalMachine.OpenSubKey(testKeyPath);
            Assert.That(testKey, Is.Null);
        }

        [Test]
        public void SaveToRegistry_SavesAffinityToImageOptions()
        {
            // Arrange
            ProcessConfigurationsRegistryManager manager = new();
            string imageName = TestContext.CurrentContext.Test.Name;
            string testKeyPath = Path.Combine(ImageOptionsFiller.ImageOptionsRegistryPath, imageName);
            ProcessConfiguration config = new(imageName)
            {
                CpuAffinityMask = 123U
            };

            try
            {
                // Act
                manager.SaveToRegistry([config]);

                // Assert
                using (RegistryKey? testKey = Registry.LocalMachine.OpenSubKey(testKeyPath))
                {
                    Assert.That(testKey, Is.Not.Null);
                    Assert.That(testKey.GetValue(ImageOptionsFiller.AffinityValueName), Is.EqualTo((long)config.CpuAffinityMask));
                }

                // Now lets try to reset and save it again
                config.Reset();
                manager.SaveToRegistry([config]);

                using (RegistryKey? testKey = Registry.LocalMachine.OpenSubKey(testKeyPath))
                {
                    Assert.That(testKey, Is.Null);
                }
            }
            finally
            {
                // Cleanup
                Registry.LocalMachine.DeleteSubKeyTree(testKeyPath, false);
            }
        }

        private static void AssertProcessConfigurationPerformance(string imageName, List<ProcessConfiguration> configurations, (CpuPriorityClass, IoPriority, PagePriority) expectedValues)
        {
            Assert.That(configurations, Is.Not.Null);
            Assert.That(configurations, Is.Not.Empty);
            ProcessConfiguration? testConfig = configurations.FirstOrDefault(c => c.Name == imageName);
            Assert.That(testConfig, Is.Not.Null);
            Assert.That(testConfig.CpuPriority, Is.EqualTo(expectedValues.Item1));
            Assert.That(testConfig.IoPriority, Is.EqualTo(expectedValues.Item2));
            Assert.That(testConfig.MemoryPriority, Is.EqualTo(expectedValues.Item3));
        }
    }
}
