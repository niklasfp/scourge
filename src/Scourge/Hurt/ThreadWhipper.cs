namespace Scourge.Hurt;

// TODO: Find a way to set thread affinity to force threads onto specific cores
// There is no exposed way of doing it without interop, see: https://github.com/dotnet/runtime/issues/21363
public static class ThreadWhipper
{
    public static int ProcessorCount => Environment.ProcessorCount;

    public static IThreadWork DoNothingWork(int threadCount, CancellationToken cancellationToken)
    {
        var threads = CreateThreadSet(threadCount);
        threads.Start(DoNothingLoop, cancellationToken);
        return threads;
    }

    private static void DoNothingLoop(int threadNum, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static ThreadWork CreateThreadSet(int threadCount)
    {
        if (threadCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threadCount), threadCount, "Invalid number of threads specified.");
        }

        return new ThreadWork(threadCount);
    }

    private class ThreadWork : IThreadWork
    {
        private readonly IList<Thread> _threads;
        private bool _isRunning;
        private readonly object _lockToken = new();

        internal ThreadWork(int threadCount)
        {
            ThreadCount = threadCount;
            _threads = new List<Thread>(threadCount);
        }

        public int ThreadCount { get; }

        public bool IsRunning
        {
            get
            {
                lock (_lockToken)
                {
                    return _isRunning;
                }
            }
        }

        public void Start(Action<int, CancellationToken> threadMethod, CancellationToken cancellationToken)
        {
            lock (_lockToken)
            {
                if (_isRunning || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                cancellationToken.Register(WaitForThreads);

                for (var i = 0; i < ThreadCount; i++)
                {
                    int threadNum = i;
                    var thread = new Thread(() => threadMethod(threadNum, cancellationToken));
                    _threads.Add(thread);
                    thread.Start();
                }

                _isRunning = true;
            }
        }

        private void WaitForThreads()
        {
            lock (_lockToken)
            {
                if (!_isRunning)
                {
                    return;
                }

                foreach (var thread in _threads)
                {
                    thread.Join();
                }

                _threads.Clear();

                _isRunning = false;
            }
        }
    }
}


