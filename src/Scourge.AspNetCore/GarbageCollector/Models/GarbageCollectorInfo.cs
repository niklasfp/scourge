namespace Scourge.AspNetCore.GarbageCollector.Models;

// ReSharper disable UnusedMember.Global
public readonly record struct GarbageCollectorInfo
{
    private readonly GCMemoryInfo _gcMemoryInfo;

    public GarbageCollectorInfo(GCMemoryInfo gcMemoryInfo) => _gcMemoryInfo = gcMemoryInfo;

    public long TotalAvailableMemoryBytes => _gcMemoryInfo.TotalAvailableMemoryBytes;

    public string TotalAvailableMemoryBytesInfo => _gcMemoryInfo.TotalAvailableMemoryBytes.ToFormattedSize();

    public long HighMemoryLoadThresholdBytes => _gcMemoryInfo.HighMemoryLoadThresholdBytes;

    public string HighMemoryLoadThresholdBytesInfo => _gcMemoryInfo.HighMemoryLoadThresholdBytes.ToFormattedSize();

    public long MemoryLoadBytes => _gcMemoryInfo.MemoryLoadBytes;

    public long HeapSizeBytes => _gcMemoryInfo.HeapSizeBytes;

    public long FragmentedBytes => _gcMemoryInfo.FragmentedBytes;

    public long Index => _gcMemoryInfo.Index;

    public int Generation => _gcMemoryInfo.Generation;

    public bool Compacted => _gcMemoryInfo.Compacted;

    public bool Concurrent => _gcMemoryInfo.Concurrent;

    public long TotalCommittedBytes => _gcMemoryInfo.TotalCommittedBytes;

    public long PromotedBytes => _gcMemoryInfo.PromotedBytes;

    public long PinnedObjectsCount => this._gcMemoryInfo.PinnedObjectsCount;

    public long FinalizationPendingCount => _gcMemoryInfo.FinalizationPendingCount;

    public IList<ulong> PauseDurationsInMs => new List<ulong>(_gcMemoryInfo.PauseDurations.ToArray().Select(p => (ulong)p.TotalMilliseconds));

    public double PauseTimePercentage => _gcMemoryInfo.PauseTimePercentage;

    public IList<GCGenerationInfo> GenerationInfo => new List<GCGenerationInfo>(_gcMemoryInfo.GenerationInfo.ToArray());

    public string DocumentationUrl => "https://learn.microsoft.com/dotnet/api/system.gcmemoryinfo";
}