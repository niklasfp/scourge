namespace Scourge.Memory;

public interface IAllocatorEntry
{
    EntryKey Key { get; init; }

    public EntryType Type { get; init; }

    public uint Size { get; init; }

    public DateTime Time { get; init; }

    public TimeSpan Age => (DateTime.UtcNow - Time);
}
