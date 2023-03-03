namespace Scourge.Hurt;

public interface IWork
{
    bool IsRunning { get; }
}

public interface IThreadWork : IWork
{
    int ThreadCount { get; }
}