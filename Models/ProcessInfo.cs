namespace ProcessManager.Core.Models;

public sealed record ProcessInfo(
    int Id,
    string Name,
    double? MemoryMb,
    string Status);