using System.Text;
using Cocona;
using static Vanara.PInvoke.Kernel32;

namespace Cpu_affinity
{
    public class ApplicationRunner
    {
        private const char space = ' ';

        public void Run(
            [Option()]
            uint affinityMask,
            [Option(StopParsingOptions = true)]
            string executable,
            [Argument]
            string[]? executableArguments)
        {
            STARTUPINFO info = new();

            CREATE_PROCESS creationFlags = CREATE_PROCESS.DEBUG_ONLY_THIS_PROCESS;
            StringBuilder stringBuilder = new(executable);
            if (executableArguments != null)
            {
                stringBuilder.Append(space);
                stringBuilder.AppendJoin(space, executableArguments);
            }

            GetProcessAffinityMask(GetCurrentProcess(), out nuint currentMask, out nuint systemMask);
            nuint affinityMaskSystem = affinityMask & systemMask;
            SetProcessAffinityMask(GetCurrentProcess(), affinityMaskSystem);
            CreateProcess(null, stringBuilder, null, null, false, creationFlags, null, null, info, out SafePROCESS_INFORMATION? pi);
            GetLastError().ThrowIfFailed();
            DebugActiveProcessStop(pi.dwProcessId);

            GetSystemInfo(out var systemInfo);
            systemInfo.dwNumberOfProcessors = 0;
        }
    }
}
