using FluentAssertions;
using Scourge.Throttling;

namespace Scourge.Tests;

public class ThrottleTests
{
    [Fact]
    public void CtrShouldValidateParameters()
    {
        var act = () => new Throttler(-1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("maxThroughputPerSecond");
    }

    [Theory]
    [InlineData(2048, 1000, 2000, 1024)]
    [InlineData(4096, 1000, 4000, 1024)]
    public void ThrottlerShouldBeAbleToCalculateSleepTime(long total, long startTime, long nowTime, int bytesPerSecond)
    {
        var sleepTime = Throttler.CalculateSleepTime(total, startTime, nowTime, bytesPerSecond);

        sleepTime.Should().Be(1000);
    }

    [Fact]
    public void NoThrottleShouldNotThrottle()
    {
        var startTimeMs = Environment.TickCount;

        Throttler.NoThrottle.ApplyThrottle(2000);

        (Environment.TickCount64 - startTimeMs).Should().BeLessThan(5);
    }


    [Fact_IgnoreOnCI]
    public void ThrottlerShouldThrottle()
    {
        var sut = new Throttler(1024);

        var startTimeMs = Environment.TickCount;

        sut.ApplyThrottle(1025);

        var elapsedMs = (double)(Environment.TickCount64 - startTimeMs);

        elapsedMs.Should().BeApproximately(1000, 30);
    }

    [Theory]
    [InlineData(1024, 0, false)]
    [InlineData(1024, 1025, true)]
    [InlineData(0, 1025, false)]
    public void NoThrottleScenarios(int maxBps, int count, bool isCancelled)
    {
        var sut = new Throttler(maxBps);
        var cts = new CancellationTokenSource();

        if (isCancelled)
        {
            cts.Cancel();
        }

        var startTimeMs = Environment.TickCount;

        sut.ApplyThrottle(count, cts.Token);

        var elapsedMs = (double)(Environment.TickCount64 - startTimeMs);

        elapsedMs.Should().BeApproximately(0, 30);
    }
}