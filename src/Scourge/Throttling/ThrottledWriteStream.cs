namespace Scourge.Throttling;

/// <summary>
/// Provides a stream that throttles writes on a base stream.
/// </summary>
/// <remarks>
/// <see cref="ThrottledWriteStream"/> does not throttle writes, it uses <see cref="Throttler.NoThrottle"/> for writes.
/// </remarks>
public sealed class ThrottledWriteStream : ThrottledReadWriteStream
{
    /// <summary>
    /// Creates a throttled instance of a stream.
    /// </summary>
    /// <param name="baseStream">The base stream that should throttled.</param>
    /// <param name="throttler">The throttler to use for writes.</param>
    /// <param name="ownsStream"><c>true</c> if this instance should own the stream and close it, otherwise <c>false</c>, <c>true</c> is the default.</param>
    public ThrottledWriteStream(Stream baseStream, IThrottler throttler, bool ownsStream = true)
        : base(baseStream, throttler, Throttler.NoThrottle, ownsStream)
    {
    }
}