namespace Scourge.Memory;

public readonly record struct AllocatorStatistics(ulong HeapSize, uint AllocationCount);
