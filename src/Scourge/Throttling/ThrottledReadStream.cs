namespace Scourge.Throttling;

/// <summary>
/// Provides a stream that throttles reads on a base stream.
/// </summary>
/// <remarks>
/// <see cref="ThrottledReadStream"/> does not throttle writes, it uses <see cref="Throttler.NoThrottle"/> for writes.
/// </remarks>
public sealed class ThrottledReadStream : ThrottledReadWriteStream
{
    /// <summary>
    /// Creates a throttled instance of a stream.
    /// </summary>
    /// <param name="baseStream">The base stream that should throttled.</param>
    /// <param name="throttler">The throttler to use for reads.</param>
    /// <param name="ownsStream"><c>true</c> If this instance owns the stream and should close it.</param>
    public ThrottledReadStream(Stream baseStream, IThrottler throttler, bool ownsStream = true)
        : base(baseStream, Throttler.NoThrottle, throttler, ownsStream)
    {
    }
}