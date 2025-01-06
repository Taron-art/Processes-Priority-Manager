using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace PPM.Application.IntegrationTests.Model.CRUD
{
    internal static class RegistryTestsHelpers
    {
        public static void BackupRegistryKey(string keyPath, string backupFilePath)
        {
            System.Diagnostics.Process process = new()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"save \"HKLM\\{keyPath}\" \"{backupFilePath}\" /y",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Assert.Fail($"Backup creation failed with {process.ExitCode}. Note that these test must run with admin rights.");
            }
        }

        public static void RestoreRegistryKey(string keyPath, string backupFilePath)
        {
            if (File.Exists(backupFilePath))
            {
                System.Diagnostics.Process process = new()
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "reg.exe",
                        Arguments = $"restore \"HKLM\\{keyPath}\" \"{backupFilePath}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    ClassicAssert.Fail($"Backup restoration failed with {process.ExitCode}. Note that these test must run with admin rights.");
                }
            }
        }
    }
}
