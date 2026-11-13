namespace ProcessManager.Core.Models;

public sealed record ProcessStartResult(
    int ProcessId,
    int? ExitCode,
    bool TimedOut);