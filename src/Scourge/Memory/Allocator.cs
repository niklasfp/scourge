namespace Scourge.Memory;

public abstract class Allocator<T> : IAllocator<T> where T : struct, IAllocatorEntry
{
    private readonly Dictionary<EntryKey, T> _heap = new();
    private readonly object _lockToken = new();
    private ulong _heapSize;
    private uint _nextId;

    protected Allocator(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public AllocatorStatistics GetStatistics()
    {
        lock (_lockToken)
        {
            return new AllocatorStatistics(_heapSize, (uint)_heap.Count);
        }
    }

    public void Clear()
    {
        lock (_lockToken)
        {
            var heapCopy = _heap.Values.ToArray();
            _heap.Clear();
            _heapSize = 0;

            foreach (var entry in heapCopy)
            {
                OnFree(entry);
            }
        }
    }


    public T Alloc(uint size)
    {

        if (size == 0)
        {
            return default;
        }


        lock (_lockToken)
        {
            var entry = OnAlloc(NextKey(), size);
            _heap.Add(entry.Key, entry);
            _heapSize += size;

            return entry;
        }
    }

    public uint Free(T entry) => Free(entry.Key);

    public uint Free(EntryKey key)
    {
        if (key.Id == 0)
        {
            return 0;
        }

        lock (_lockToken)
        {

            if (!_heap.Remove(key, out var entry))
            {
                return 0;
            }

            OnFree(entry);
            _heapSize -= entry.Size;
            return entry.Size;

        }
    }

    public bool TryGetEntry(EntryKey key, out T entry)
    {
        lock (_lockToken)
        {
            var result = _heap.TryGetValue(key, out var entryResult);
            entry = entryResult!;

            return result;
        }
    }

    public IReadOnlyList<T> GetAllEntries()
    {
        lock (_lockToken)
        {
            return _heap.Values.ToList().AsReadOnly();
        }
    }
    private EntryKey NextKey()
    {
        var id = Interlocked.Increment(ref _nextId);
        return new EntryKey(id);
    }

    protected abstract T OnAlloc(EntryKey key, uint size);


    protected abstract void OnFree(T value);
}