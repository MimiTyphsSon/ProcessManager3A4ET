using System.Runtime.InteropServices;

namespace ProcessManager.Core.Services;

public static class CurrentProcessIdProvider
{
    [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcessId")]
    private static extern uint GetCurrentProcessIdNative();

    public static int GetCurrentProcessId()
    {
        if (OperatingSystem.IsWindows())
        {
            return (int)GetCurrentProcessIdNative();
        }

        return Environment.ProcessId;
    }
}