using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Affinity_manager.Model;
using Affinity_manager.Model.CRUD;
using Microsoft.Win32;
using NUnit.Framework;

namespace PPM.Application.IntegrationTests.Model.CRUD
{
    [TestFixture]
    public class ProcessConfigurationRepositoryTests
    {
        private const string IfeoRegistryKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";
        private const string AppOptionsRegistryKeyPath = @"SOFTWARE\Processes Priority Manager";

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
        public void CleanWithoutServiceRestart_ThenAllRelatedItemsFromRegistryAreRemoved()
        {
            ProcessConfiguration configuration = new(TestContext.CurrentContext.Random.GetString() + ".exe")
            {
                IoPriority = IoPriority.Low
            };

            ProcessConfiguration configuration1 = new(TestContext.CurrentContext.Random.GetString() + ".exe")
            {
                CpuPriority = CpuPriorityClass.High,
                CpuAffinityMask = 1234
            };

            ProcessConfiguration configuration2 = new(TestContext.CurrentContext.Random.GetString() + ".exe")
            {
                CpuAffinityMask = 1U
            };

            ProcessConfigurationsRepository repository = new();
            repository.Save([configuration, configuration1, configuration2]);

            repository.CleanWithoutServiceRestart();

            Assert.That(repository.Get(), Is.Empty);
        }

        [Test]
        public void SaveAndRestartServiceAsync_ThrowsIfInvalid()
        {
            ProcessConfiguration configuration = new(TestContext.CurrentContext.Random.GetString())
            {
                CpuAffinityMask = 1U
            };

            ProcessConfigurationsRepository repository = new();
            Assert.ThrowsAsync<ValidationException>(() => repository.SaveAndRestartServiceAsync([configuration]));
        }
    }
}
