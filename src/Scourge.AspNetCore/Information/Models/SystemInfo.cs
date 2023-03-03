namespace Scourge.AspNetCore.Information.Models;

// ReSharper disable UnusedMember.Global
public record SystemInfo(string MachineName, long TotalMemoryBytes, int ProcessorCount)
{
    public string TotalMemory => TotalMemoryBytes.ToFormattedSize();

    public string SystemUpTime => TimeSpan.FromMilliseconds(Environment.TickCount64).ToFormattedDuration();
}