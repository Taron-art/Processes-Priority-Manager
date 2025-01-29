using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace PPM.Unsafe
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly partial struct NTSTATUS
    {
        [LibraryImport(Lib.NtDll)]
        private static partial ulong RtlNtStatusToDosError(nint ntstatus);

        public readonly nint Value;

        public int Win32ErrorCode
        {
            get
            {
                unchecked
                {
                    return (int)RtlNtStatusToDosError(Value);
                }
            }
        }
    }
}
