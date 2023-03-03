using System.Diagnostics;

namespace Scourge.AspNetCore.Information.Models;

// ReSharper disable UnusedMember.Global
public record MemoryInfo(long WorkingSet, long PrivateMemorySize, long MaxWorkingSet, long MinWorkingSet,
    long VirtualMemorySize, long PagedMemorySize, long PeakWorkingSet, long NonpagedSystemMemorySize)
{
    public string PrivateMemorySizeInfo => PrivateMemorySize.ToFormattedSize();
    public string WorkingSetInfo => WorkingSet.ToFormattedSize();
    public string MinWorkingSetInfo => MinWorkingSet.ToString();
    public string MaxWorkingSetInfo => MaxWorkingSet.ToString();
    public string PeakWorkingSetInfo => PeakWorkingSet.ToFormattedSize();
    public string VirtualMemorySizeInfo => VirtualMemorySize.ToFormattedSize();
    public string NonpagedSystemMemorySizeInfo => NonpagedSystemMemorySize.ToFormattedSize();

    public static MemoryInfo FromProcess(Process process)
    {
        return new MemoryInfo(
            process.WorkingSet64,
            process.PrivateMemorySize64,
            process.MaxWorkingSet,
            process.MinWorkingSet,
            process.VirtualMemorySize64,
            process.PagedMemorySize64,
            process.PeakWorkingSet64,
            process.NonpagedSystemMemorySize64);
    }
}