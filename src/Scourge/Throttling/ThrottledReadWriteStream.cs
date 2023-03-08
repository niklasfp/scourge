namespace Scourge.Throttling;

/// <summary>
/// Provides a stream that throttles reads and writes on base stream.
/// </summary>
public class ThrottledReadWriteStream : Stream
{
    private readonly Stream _baseStream;
    private readonly IThrottler _writeThrottler;
    private readonly IThrottler _readThrottler;
    private readonly bool _ownsStream;

    /// <summary>
    /// Creates a throttled instance of a stream.
    /// </summary>
    /// <param name="baseStream">The base stream that should throttled.</param>
    /// <param name="writeThrottler">The throttler to use for writes, if not throttling is needed for writes, pass in <see cref="Throttler.NoThrottle"/> as parameter value.</param>
    /// <param name="readThrottler">The throttler to use for reads, if not throttling is needed for reads, pass in <see cref="Throttler.NoThrottle"/> as parameter value.</param>
    /// <param name="ownsStream"><c>true</c> If this instance owns the stream and should close it.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="baseStream"/>, <paramref name="writeThrottler"/> or <paramref name="readThrottler"/> is <c>null</c>.</exception>
    /// <remarks>
    /// If throttling is needed to be one throttle for both reads and writes, then construct an instance with the same <see cref="IThrottler"/> instance passed to both <paramref name="writeThrottler"/> or <paramref name="readThrottler"/>.
    /// </remarks>
    public ThrottledReadWriteStream(Stream baseStream, IThrottler writeThrottler, IThrottler readThrottler, bool ownsStream = true)
    {
        _baseStream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
        _writeThrottler = writeThrottler ?? throw new ArgumentNullException(nameof(writeThrottler));
        _readThrottler = readThrottler ?? throw new ArgumentNullException(nameof(readThrottler));

        _ownsStream = ownsStream;
    }

    /// <summary>Gets a value indicating whether the underlying stream supports reading.</summary>
    public override bool CanRead => _baseStream.CanRead;

    /// <summary>Gets a value indicating whether the underlying stream supports seeking.</summary>
    public override bool CanSeek => _baseStream.CanSeek;

    /// <summary>Gets a value indicating whether the underlying stream supports writing.</summary>
    public override bool CanWrite => _baseStream.CanWrite;

    /// <summary>
    /// Gets the length in bytes of the underlying stream.
    /// </summary>
    /// <returns>A long value representing the length of the underlying stream in bytes.</returns>
    public override long Length => _baseStream.Length;

    /// <summary>Gets or sets the position within the underlying stream</summary>
    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    /// <summary>
    /// Clears buffers for the underlying stream and causes any buffered data to be written to the underlying device.
    /// </summary>
    public override void Flush() => _baseStream.Flush();

    /// <summary>
    /// Asynchronously clears all buffers for the underlying stream and causes any buffered data to be written to the underlying device.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public override Task FlushAsync(CancellationToken cancellationToken) => _baseStream.FlushAsync(cancellationToken);

    /// <summary>
    /// Reads a sequence of bytes from the underlying stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the underlying stream.</param>
    /// <param name="count">The maximum number of bytes to be read from the underlying stream.</param>
    /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
    public override int Read(byte[] buffer, int offset, int count)
    {
        _readThrottler.ApplyThrottle(count);
        return _baseStream.Read(buffer, offset, count);
    }

    /// <summary>
    /// Asynchronously reads a sequence of bytes from the underlying stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the underlying stream.</param>
    /// <param name="count">The maximum number of bytes to be read from the underlying stream.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
    /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        _readThrottler.ApplyThrottle(count, cancellationToken);
        return _baseStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    /// <summary>
    /// Asynchronously reads a sequence of bytes from the underlying stream and advances the position within the stream by the number of bytes read.
    /// </summary>
    /// <param name="buffer">The region of memory to write the data into.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
    /// <returns>A task that represents the asynchronous read operation. The value of its <see cref="P:System.Threading.Tasks.ValueTask`1.Result" /> property contains the total number of bytes read into the buffer. The result value can be less than the number of bytes allocated in the buffer if that many bytes are not currently available, or it can be 0 (zero) if the end of the stream has been reached.</returns>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        _readThrottler.ApplyThrottle(buffer.Length, cancellationToken);
        return _baseStream.ReadAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// Sets the position within the underlying stream.
    /// </summary>
    /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
    /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
    /// <returns>The new position within the underlying stream.</returns>
    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

    /// <summary>
    /// Sets the length of the underlying stream.
    /// </summary>
    /// <param name="value">The desired length of the underlying stream in bytes.</param>
    public override void SetLength(long value) => _baseStream.SetLength(value);

    /// <summary>
    /// Writes a sequence of bytes to the underlying stream and advances the current position within this stream by the number of bytes written.
    /// </summary>
    /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the underlying stream.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the underlying stream.</param>
    /// <param name="count">The number of bytes to be written to the underlying stream.</param>
    public override void Write(byte[] buffer, int offset, int count)
    {
        _writeThrottler.ApplyThrottle(count);
        _baseStream.Write(buffer, offset, count);
    }

    /// <summary>
    /// Asynchronously writes a sequence of bytes to the underlying stream, advances the current position within the underlying stream by the number of bytes written, and monitors cancellation requests.
    /// </summary>
    /// <param name="buffer">The buffer to write data from.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the underlying stream.</param>
    /// <param name="count">The maximum number of bytes to write.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        _writeThrottler.ApplyThrottle(count, cancellationToken);
        return _baseStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    /// <summary>Asynchronously writes a sequence of bytes to the underlying stream, advances the current position within underlying stream by the number of bytes written, and monitors cancellation requests.</summary>
    /// <param name="buffer">The region of memory to write data from.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        _writeThrottler.ApplyThrottle(buffer.Length, cancellationToken);
        return _baseStream.WriteAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// Closes the underlying stream if this instance owns the stream.
    /// </summary>
    public override void Close()
    {
        if (_ownsStream)
        {
            _baseStream.Close();
        }

        base.Close();
    }
}