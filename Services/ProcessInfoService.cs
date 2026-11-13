using System.Diagnostics;
using System.ComponentModel;
using ProcessManager.Core.Models;

namespace ProcessManager.Core.Services;

public sealed class ProcessInfoService : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    public ProcessInfoService(int maxConcurrency = 4)
    {
        _semaphore = new SemaphoreSlim(maxConcurrency);
    }

    public async Task<List<ProcessInfo>> GetProcessesAsync(
        CancellationToken cancellationToken = default)
    {
        Process[] processes = await Task.Run(
            Process.GetProcesses,
            cancellationToken);

        try
        {
            var tasks = processes.Select(
                process => ReadWithLimitAsync(process, cancellationToken));

            return (await Task.WhenAll(tasks)).ToList();
        }
        finally
        {
            foreach (Process process in processes)
            {
                process.Dispose();
            }
        }
    }

    private async Task<ProcessInfo> ReadWithLimitAsync(
        Process process,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            return await Task.Run(
                () => ReadProcess(process),
                CancellationToken.None);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static ProcessInfo ReadProcess(Process process)
    {
        int id = -1;
        string name = "Недоступно";
        double? memory = null;
        string status = "Недоступно";

        try
        {
            id = process.Id;
            name = process.ProcessName;

            if (process.HasExited)
            {
                status = "Завершён";
            }
            else
            {
                status = "Работает";

                try
                {
                    memory = process.WorkingSet64 / 1024.0 / 1024.0;
                }
                catch (Exception ex) when (
                    ex is InvalidOperationException ||
                    ex is Win32Exception ||
                    ex is NotSupportedException)
                {
                    memory = null;
                }
            }
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is Win32Exception ||
            ex is NotSupportedException ||
            ex is ObjectDisposedException)
        {
            status = "Недоступен";
        }

        return new ProcessInfo(id, name, memory, status);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _semaphore.Dispose();
        _disposed = true;
    }
}