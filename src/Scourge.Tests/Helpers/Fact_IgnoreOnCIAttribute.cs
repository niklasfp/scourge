namespace Scourge.Tests.Helpers;

// See: https://stackoverflow.com/questions/69196696/skip-unit-tests-while-running-on-the-build-server
public class Fact_IgnoreOnCIAttribute : FactAttribute
{
    public override string Skip
    {
        get => IsRunningOnBuildServer() ? "This integration test is skipped running in with 'CI' env var set, Run it locally!" : null!;
        set { }
    }

    /// <summary>
    /// Determine if the test is running on build server
    /// </summary>
    /// <returns>True if being executed in Build server, false otherwise.</returns>
    private static bool IsRunningOnBuildServer()
    {
        // CI is at least set by GitHub.
        return Environment.GetEnvironmentVariable("CI") != null;
    }
}