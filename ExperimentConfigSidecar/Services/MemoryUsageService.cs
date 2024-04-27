using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExperimentConfigSidecar.Services
{
    public class MemoryUsageService
    {

        private IntPtr memoryHandle = IntPtr.Zero;

        private long memoryUsage = 0;

        private readonly object lockObject = new();

        public void UpdateMemoryUsage(long bytes)
        {
            lock (lockObject)
            {
                if (memoryUsage != bytes)
                {
                    memoryUsage = bytes;
                    if (memoryHandle != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(memoryHandle);
                    }
                    memoryHandle = Marshal.AllocHGlobal((nint)bytes);
                    InitMemory(bytes);
                }
            }
        }

        private void InitMemory(long bytes)
        {
            unsafe
            {
                nint toInit = (nint)bytes;
                nint allocated = 0;
                while (toInit > 0)
                {
                    uint toInitCurrently = (uint)Math.Min(toInit, uint.MaxValue);
                    Unsafe.InitBlockUnaligned((void*)(memoryHandle + allocated), 1, toInitCurrently);
                    toInit -= (nint)toInitCurrently;
                    allocated += (nint)toInitCurrently;
                }
                Unsafe.InitBlockUnaligned((void*)memoryHandle, 1, (uint)bytes);
            }
        }
    }
}