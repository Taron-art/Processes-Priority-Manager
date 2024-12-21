using System;
using System.IO;
using System.Reflection;

namespace Affinity_manager.Model
{
    internal class DebuggerKeyManager
    {
        private const char Separator = ' ';
        private const string AffinityMaskCommandLineArg = "--affinity-mask";
        private const string ExecutableCommandLineArg = "--executable";
        private const string RunnerName = "Cpu_affinity.exe";
        private const string DebuggerKeyFormat = "\"{0}\" " + AffinityMaskCommandLineArg + " {1} " + ExecutableCommandLineArg;


        public bool TryParseDebugLine(string debugLine, out uint affinity)
        {
            affinity = uint.MaxValue;
            var commandLineArguments = debugLine.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

            int? affinityMaskPosition = null;
            for (int i = 0; i < commandLineArguments.Length; i++)
            {
                if (commandLineArguments[i].Equals(AffinityMaskCommandLineArg, StringComparison.OrdinalIgnoreCase) && i != commandLineArguments.Length)
                {
                    affinityMaskPosition = i + 1;
                    break;
                }
            }

            if (affinityMaskPosition == null)
            {
                return false;
            }

            try
            {
                affinity = Convert.ToUInt32(commandLineArguments[affinityMaskPosition.Value], 16);
            }
            catch (FormatException)
            {
                // If format is incorrect, just ignore it and use max value.
            }
            catch (OverflowException)
            {
                // If result value is overflow, just ignore it and use max value.
            }

            return true;
        }

        public string CreateDebuggerLine(uint affinity)
        {
            if (affinity == 0 || affinity == uint.MaxValue)
            {
                return string.Empty;
            }

            string? pathToCurrentExecutable = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string pathToDebugger = pathToCurrentExecutable == null ? RunnerName : Path.Combine(pathToCurrentExecutable, RunnerName);

            return string.Format(DebuggerKeyFormat, pathToDebugger, $"0x{affinity:X}");
        }
    }
}
