using Scourge.Memory;

namespace Scourge.AspNetCore.Memory.Models;

public record MemoryEntryModel(uint Key, string Type, uint Size, DateTime Time, uint AgeInSeconds)
{
    public static MemoryEntryModel Map(IAllocatorEntry entry)
    {
        return new MemoryEntryModel(entry.Key.Id, entry.Type.ToString(), entry.Size, entry.Time, (uint)entry.Age.TotalSeconds);
    }
}

public record AllocatorInfo(ulong AllocatedBytes, uint AllocationCount)
{
    public static AllocatorInfo Map(AllocatorStatistics stats)
    {
        return new AllocatorInfo(stats.HeapSize, stats.AllocationCount);
    }
}

public record AllAllocatorsInfo(AllocatorInfo Managed, AllocatorInfo Unmanaged)
{
    public static AllAllocatorsInfo Map(AllocatorStatistics managed, AllocatorStatistics unmanaged)
    {
        return new AllAllocatorsInfo(
            Managed: new AllocatorInfo(managed.HeapSize, managed.AllocationCount),
            Unmanaged: new AllocatorInfo(unmanaged.HeapSize, unmanaged.AllocationCount));
    }
}
