using System.Diagnostics;
using ProcessManager.Core.Models;

namespace ProcessManager.Core.Services;

public sealed class ProcessLauncher
{
    public async Task<ProcessStartResult> StartAsync(
        string executablePath,
        string arguments,
        TimeSpan? timeout = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false
        };

        foreach (string argument in SplitArguments(arguments))
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process? process = Process.Start(startInfo);

        if (process == null)
        {
            throw new InvalidOperationException(
                "Не удалось запустить программу.");
        }

        int processId = process.Id;

        if (timeout == null)
        {
            return new ProcessStartResult(processId, null, false);
        }

        using var cancellation = new CancellationTokenSource(timeout.Value);

        try
        {
            await process.WaitForExitAsync(cancellation.Token);

            return new ProcessStartResult(
                processId,
                process.ExitCode,
                false);
        }
        catch (OperationCanceledException)
        {
            return new ProcessStartResult(
                processId,
                null,
                true);
        }
    }

    private static List<string> SplitArguments(string arguments)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();

        bool inQuotes = false;

        foreach (char character in arguments)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(character) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(character);
            }
        }

        if (inQuotes)
        {
            throw new ArgumentException(
                "Не закрыта кавычка в аргументах.");
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }
}