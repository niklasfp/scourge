namespace Scourge.AspNetCore.Throttling.Middleware;

public class ThrottlingMiddlewareConfigService
{
    private bool _enabled;
    private int _maxBytesPerSecond = 50 * 1024; // Default to 50Kb/sec
    

    public bool Enabled
    {
        get => _enabled;
        set => _enabled = value;
    }

    public int MaxBytesPerSecond
    {
        get => _maxBytesPerSecond;
        set => _maxBytesPerSecond = value;
    }
}