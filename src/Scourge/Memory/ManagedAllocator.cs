using Scourge.Diagnostics;

namespace Scourge.Memory;

public sealed class ManagedAllocator : Allocator<ManagedAllocatorEntry>
{
    public ManagedAllocator()
        : base("Managed Allocator")
    {
    }

    protected override ManagedAllocatorEntry OnAlloc(EntryKey key, uint size)
    {
        var result = new ManagedAllocatorEntry(key, size, new byte[size]);
        Metrics.AddManagedAllocatedOrDeallocated(size);

        return result;
    }

    protected override void OnFree(ManagedAllocatorEntry value)
    {
        // The removal of the entry from the heap will release the heap for garbage collection.
        // So we just record the size we "freed"
        Metrics.AddManagedAllocatedOrDeallocated(-value.Size);
    }
}