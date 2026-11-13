using System.ComponentModel;
using System.Diagnostics;
using ProcessManager.Core.Services;

namespace Variant01.Services;

public sealed class ProjectService : IDisposable
{
    private readonly ProcessInfoService _processInfo = new(4);
    private readonly ProcessLauncher _launcher = new();
    private readonly RunHistoryService _history = new();
    private readonly ActionLogger _logger = new();

    public async Task RunAsync()
    {
        Console.WriteLine($"PID текущего приложения: {CurrentProcessIdProvider.GetCurrentProcessId()}");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1. Показать запущенные процессы");
            Console.WriteLine("2. Запустить программу");
            Console.WriteLine("3. Показать историю запусков");
            Console.WriteLine("4. Показать журнал действий");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите действие: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ShowProcessesAsync();
                    break;

                case "2":
                    await StartProgramAsync();
                    break;

                case "3":
                    ShowHistory();
                    break;

                case "4":
                    ShowLog();
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Неверный пункт меню.");
                    break;
            }
        }
    }

    private async Task ShowProcessesAsync()
    {
        Console.WriteLine("\nПолучение списка процессов...");

        var processes = await _processInfo.GetProcessesAsync();

        Console.WriteLine(
            $"{"PID",-10} {"Имя",-30} {"Память (МБ)",-18} {"Статус"}");

        Console.WriteLine(new string('-', 75));

        foreach (var process in processes)
        {
            string memory = process.MemoryMb?.ToString("F2") ?? "Недоступно";

            Console.WriteLine(
                $"{process.Id,-10} {process.Name,-30} {memory,-18} {process.Status}");
        }

        Console.WriteLine($"\nВсего процессов: {processes.Count}");
    }

    private async Task StartProgramAsync()
    {
        Console.Write("\nВведите путь к программе: ");
        string? path = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("Путь не указан.");
            return;
        }

        Console.Write("Введите аргументы запуска (можно оставить пустым): ");
        string? arguments = Console.ReadLine();

        Console.Write("Тайм-аут ожидания в секундах (0 — не ждать): ");

        if (!int.TryParse(Console.ReadLine(), out int seconds) || seconds < 0)
        {
            Console.WriteLine("Некорректный тайм-аут.");
            return;
        }

        try
        {
            TimeSpan? timeout = seconds == 0
                ? null
                : TimeSpan.FromSeconds(seconds);

            var result = await _launcher.StartAsync(
                path,
                arguments ?? "",
                timeout);

            _history.Add(path);

            _logger.Write(
                $"Запуск программы: {path}, PID: {result.ProcessId}");

            Console.WriteLine($"Программа запущена. PID: {result.ProcessId}");

            if (result.TimedOut)
            {
                Console.WriteLine("Время ожидания истекло. Процесс продолжает работу.");
            }
            else if (result.ExitCode.HasValue)
            {
                Console.WriteLine($"Код завершения: {result.ExitCode}");
            }
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is Win32Exception ||
            ex is ArgumentException ||
            ex is FileNotFoundException)
        {
            Console.WriteLine($"Ошибка запуска: {ex.Message}");
            _logger.Write($"Ошибка запуска {path}: {ex.Message}");
        }
    }

    private void ShowHistory()
    {
        Console.WriteLine("\nПоследние пути запуска:");

        var paths = _history.GetRecent();

        if (paths.Count == 0)
        {
            Console.WriteLine("История пуста.");
            return;
        }

        foreach (string path in paths)
        {
            Console.WriteLine(path);
        }
    }

    private void ShowLog()
    {
        Console.WriteLine("\nЖурнал действий:");

        var entries = _logger.ReadAll();

        if (entries.Count == 0)
        {
            Console.WriteLine("Журнал пуст.");
            return;
        }

        foreach (string entry in entries)
        {
            Console.WriteLine(entry);
        }
    }

    public void Dispose()
    {
        _processInfo.Dispose();
    }
}