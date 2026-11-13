using System.Text;
using Variant01.Services;

namespace Variant01;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Зачётный проект 01: Диспетчер запущенных процессов");
        Console.WriteLine(new string('=', 64));

        using var service = new ProjectService();

        await service.RunAsync();

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для завершения.");
        Console.ReadLine();
    }
}