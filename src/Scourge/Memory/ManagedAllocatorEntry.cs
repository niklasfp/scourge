namespace Scourge.Memory;

public readonly record struct ManagedAllocatorEntry(EntryKey Key, uint Size, byte[] Data) : IAllocatorEntry
{
    public EntryType Type { get; init; } = EntryType.Managed;

    public DateTime Time { get; init; } = DateTime.UtcNow;
}
