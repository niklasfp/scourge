using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scourge.Hurt
{
    public readonly record struct WorkInfo(string Id, DateTimeOffset StartTime, IWork Work);

    public interface IWorkManager
    {
        WorkInfo StartWork(Func<CancellationToken, IWork> createWork);

        public bool StopWork(string id);

        IEnumerable<WorkInfo> GetActiveWork();

        public static IWorkManager CreateDefault() => new WorkManager();
    }

    // Needs some work.
    // currently work finishing without being cancelled will remain in work in progress
    // And for now we do not really care, all work we schedule running until cancelled.
    internal class WorkManager : IWorkManager
    {
        private readonly ConcurrentDictionary<string, WorkEntry> _workInProgress = new();

        public WorkInfo StartWork(Func<CancellationToken, IWork> createWork)
        {
            var id = Guid.NewGuid().ToString("N");
            var cts = new CancellationTokenSource();

            var work = createWork(cts.Token);
            var workEntry = new WorkEntry(id, DateTime.UtcNow, work, cts);

            _workInProgress.TryAdd(id, workEntry);
            return WorkEntryToInfo(workEntry);
        }

        public bool StopWork(string id)
        {
            if (_workInProgress.TryRemove(id, out var entry))
            {
                entry.TokenSource.Cancel();
                return true;
            }

            return false;
        }

        public IEnumerable<WorkInfo> GetActiveWork()
        {
            return _workInProgress.Select(e => WorkEntryToInfo(e.Value));
        }

        private static WorkInfo WorkEntryToInfo(WorkEntry entry)
        {
            return new WorkInfo(entry.Id, entry.StartTime, entry.Work);
        }

        private record struct WorkEntry(string Id, DateTimeOffset StartTime, IWork Work, CancellationTokenSource TokenSource);
    }
}
