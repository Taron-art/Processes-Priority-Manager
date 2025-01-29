using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Vanara.PInvoke;

namespace PPM.Unsafe
{
    public static partial class ProcessExtenstions
    {
        [LibraryImport(Lib.NtDll)]
        private static partial NTSTATUS NtSetInformationProcess(
            IntPtr ProcessHandle,
            ProcessInfoClass ProcessInformationClass,
            ref IntPtr ProcessInformation,
            uint ProcessInformationLength);

        [LibraryImport(Lib.NtDll)]
        private static partial NTSTATUS NtQueryInformationProcess(
            IntPtr ProcessHandle,
            ProcessInfoClass ProcessInformationClass,
            ref IntPtr ProcessInformation,
            uint ProcessInformationLength,
            out uint ReturnLength);


        /// <summary>
        /// Sets the I/O priority of the specified process.
        /// </summary>
        /// <param name="process">Process to set I/O priority.</param>
        /// <param name="priority">I/O priority value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="process"/> is <see langword="null"/>.</exception>"
        /// <exception cref="InvalidOperationException">If process exited.</exception>
        /// <exception cref="Win32Exception">If set of priority failed.</exception>"
        public static void SetIoPriority(this Process process, in IoPriorityHint priority)
        {
            ArgumentNullException.ThrowIfNull(process, nameof(process));

            nint internalPriority = (IntPtr)priority;
            var result = NtSetInformationProcess(process.Handle, ProcessInfoClass.ProcessIoPriority, ref internalPriority, sizeof(IoPriorityHint));

            if (result.Value != 0)
            {
                throw new Win32Exception(result.Win32ErrorCode);
            }
        }

        /// <summary>
        /// Gets the I/O priority of the specified process.
        /// </summary>
        /// <param name="process">Process to get I/O priority.</param>
        /// <returns>I/O priority of the process.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="process"/> is <see langword="null"/>.</exception>""
        /// <exception cref="InvalidOperationException">If get of priority failed.</exception>
        /// <exception cref="Win32Exception">If get of priority failed.</exception>"
        public static IoPriorityHint GetIoPriority(this Process process)
        {
            ArgumentNullException.ThrowIfNull(process, nameof(process));
            nint internalPriority = 0;
            NTSTATUS result = NtQueryInformationProcess(process.Handle, ProcessInfoClass.ProcessIoPriority, ref internalPriority, sizeof(IoPriorityHint), out _);
            if (result.Value != 0)
            {
                throw new Win32Exception(result.Win32ErrorCode);
            }
            return (IoPriorityHint)internalPriority;
        }

        /// <summary>
        /// Sets the memory priority of the specified process.
        /// </summary>
        /// <param name="process">Process to set memory (page) priority.</param>
        /// <param name="priority">Memory priority value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="process"/> is <see langword="null"/>.</exception>"
        /// <exception cref="InvalidOperationException">If set of priority failed.</exception>
        /// <exception cref="Win32Exception">If set of priority failed.</exception>""
        public static void SetMemoryPriority(this Process process, in PagePriorityInformation priority)
        {
            ArgumentNullException.ThrowIfNull(process, nameof(process));

            nint internalPriority = (IntPtr)priority;
            NTSTATUS result = NtSetInformationProcess(process.Handle, ProcessInfoClass.ProcessPagePriority, ref internalPriority, sizeof(PagePriorityInformation));
            if (result.Value != 0)
            {
                throw new Win32Exception(result.Win32ErrorCode);
            }
        }

        /// <summary>
        /// Gets the memory priority of the specified process.
        /// </summary>
        /// <param name="process">Process to get memory priority.</param>
        /// <returns>Memory priority of the process.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="process"/> is <see langword="null"/>.</exception>""
        /// <exception cref="InvalidOperationException">If get of priority failed.</exception>
        /// <exception cref="Win32Exception">If get of priority failed.</exception>""
        public static PagePriorityInformation GetMemoryPriority(this Process process)
        {
            ArgumentNullException.ThrowIfNull(process, nameof(process));

            nint internalPriority = 0;
            NTSTATUS result = NtQueryInformationProcess(process.Handle, ProcessInfoClass.ProcessPagePriority, ref internalPriority, sizeof(PagePriorityInformation), out _);
            if (result.Value != 0)
            {
                throw new Win32Exception(result.Win32ErrorCode);
            }
            return (PagePriorityInformation)internalPriority;
        }
    }
}
