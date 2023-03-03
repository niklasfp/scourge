namespace Scourge.Memory;

public readonly record struct UnmanagedAllocatorEntry(EntryKey Key, uint Size, IntPtr Ptr) : IAllocatorEntry
{
    public EntryType Type { get; init; } = EntryType.Unmanaged;

    public DateTime Time { get; init; } = DateTime.UtcNow;
}
