using System.Diagnostics;

namespace Scourge.AspNetCore.Information.Models;

// ReSharper disable UnusedMember.Global
public class ProcessInfo
{
    private readonly Process _process;

    public ProcessInfo(Process process) => _process = process;

    public int Id => _process.Id;

    public string Name => _process.ProcessName;

    public int SessionId => _process.SessionId;

    public DateTimeOffset StartTime => _process.StartTime;

    public MemoryInfo Memory => MemoryInfo.FromProcess(_process);

    public int UpTimeMinutes => (int)Math.Round((DateTime.Now - _process.StartTime).TotalMinutes);

    public string UpTime => (DateTime.Now - _process.StartTime).ToFormattedDuration();
}