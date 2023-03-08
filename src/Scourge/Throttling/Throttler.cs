using System.Runtime.CompilerServices;

namespace Scourge.Throttling;

public sealed class Throttler : IThrottler
{
    public static readonly IThrottler NoThrottle = new NoOpThrottler();

    private long _total;
    private long _startTimeMs;
    private readonly int _maxThroughputPerSecond;

    public Throttler(int maxThroughputPerSecond)
    {
        if (maxThroughputPerSecond < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxThroughputPerSecond), maxThroughputPerSecond, "Must be a positive integer");
        }

        _maxThroughputPerSecond = maxThroughputPerSecond;
    }

    public void ApplyThrottle(int count, CancellationToken cancellationToken = default)
    {
        if (_maxThroughputPerSecond <= 0 || count <= 0 || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var nowMs = Environment.TickCount64;

        var currentTotal = Interlocked.Add(ref _total, count);
        Interlocked.CompareExchange(ref _startTimeMs, nowMs, 0);

        var sleepTimeMs = CalculateSleepTime(currentTotal, _startTimeMs, nowMs, _maxThroughputPerSecond);

        if (sleepTimeMs > 0)
        {
            cancellationToken.WaitHandle.WaitOne(sleepTimeMs);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int CalculateSleepTime(long total, long startTimeMs, long nowMs, int maxThroughputPerSec)
    {
        var elapsed = nowMs - startTimeMs;
        var targetTimeMs = (double)total / maxThroughputPerSec * 1000;
        return (int)(targetTimeMs - elapsed);
    }

    private sealed class NoOpThrottler : IThrottler
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ApplyThrottle(int count, CancellationToken cancellationToken = default)
        {
            // Do nothing
        }
    }
}