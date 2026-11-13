using Microsoft.Win32;
using System.Security;

namespace Variant01.Services;

public sealed class RunHistoryService
{
    private const string RegistryPath =
        @"Software\Variant01\ProcessManager";

    private const string ValueName = "RecentPaths";

    public void Add(string path)
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            using RegistryKey key = Registry.CurrentUser.CreateSubKey(
                RegistryPath);

            string[] oldPaths =
                key.GetValue(ValueName) as string[] ?? [];

            string[] newPaths = new[] { path }
                .Concat(oldPaths.Where(p => p != path))
                .Take(5)
                .ToArray();

            key.SetValue(ValueName, newPaths, RegistryValueKind.MultiString);
        }
        catch (Exception ex) when (
            ex is SecurityException ||
            ex is UnauthorizedAccessException)
        {
            Console.WriteLine($"Не удалось сохранить историю: {ex.Message}");
        }
    }

    public List<string> GetRecent()
    {
        if (!OperatingSystem.IsWindows())
            return [];

        try
        {
            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(RegistryPath);

            return key?.GetValue(ValueName) as string[] is { } paths
                ? paths.ToList()
                : [];
        }
        catch (Exception ex) when (
            ex is SecurityException ||
            ex is UnauthorizedAccessException)
        {
            return [];
        }
    }
}