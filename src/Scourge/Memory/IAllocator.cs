namespace Scourge.Memory;

public interface IAllocator
{
    string Name { get; }
    AllocatorStatistics GetStatistics();
    void Clear();
    uint Free(EntryKey key);
}

public interface IAllocator<T> : IAllocator where T : struct, IAllocatorEntry
{
    T Alloc(uint size);
    IReadOnlyList<T> GetAllEntries();
    bool TryGetEntry(EntryKey key, out T entry);
    uint Free(T entry);
}