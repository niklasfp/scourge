namespace Scourge.Throttling;

public interface IThrottler
{
    void ApplyThrottle(int count, CancellationToken cancellationToken = default);
}