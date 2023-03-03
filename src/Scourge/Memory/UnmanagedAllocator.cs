using System.Runtime.InteropServices;
using Scourge.Diagnostics;

namespace Scourge.Memory;

public sealed class UnmanagedAllocator : Allocator<UnmanagedAllocatorEntry>
{
    public UnmanagedAllocator()
        : base("Unmanaged Allocator")
    {
    }

    protected override unsafe UnmanagedAllocatorEntry OnAlloc(EntryKey key, uint size)
    {
        // Consider making a version with safe handles
        // Downside is that this will rely on GC
        var result = new UnmanagedAllocatorEntry(key, size, (nint)NativeMemory.Alloc(size));
        Metrics.AddUnManagedAllocatedOrDeallocated(size);

        return result;
    }

    protected override unsafe void OnFree(UnmanagedAllocatorEntry value)
    {
        // Free Native memory
        NativeMemory.Free((void*)value.Ptr);

        // Record it.
        Metrics.AddUnManagedAllocatedOrDeallocated(-value.Size);
    }
}