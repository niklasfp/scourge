using System.Runtime.InteropServices;
#if WITH_DIAG
using Scourge.Diagnostics;
#endif

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
#if WITH_DIAG
        Metrics.AddUnManagedAllocatedOrDeallocated(size);
#endif

        return result;
    }

    protected override unsafe void OnFree(UnmanagedAllocatorEntry value)
    {
        // Free Native memory
        NativeMemory.Free((void*)value.Ptr);

#if WITH_DIAG
        // Record it.
        Metrics.AddUnManagedAllocatedOrDeallocated(-value.Size);
#endif
    }
}