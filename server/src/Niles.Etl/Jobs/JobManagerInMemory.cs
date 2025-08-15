using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Niles.Etl.Jobs
{
    public sealed class JobManagerInMemory : IJobManager
    {
        private readonly Channel<JobWorkItem> _queue;
        private readonly ConcurrentDictionary<string, JobStatus> _jobs;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens;

        public JobManagerInMemory()
        {
            _queue = Channel.CreateUnbounded<JobWorkItem>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            _jobs = new ConcurrentDictionary<string, JobStatus>(StringComparer.OrdinalIgnoreCase);
            _tokens = new ConcurrentDictionary<string, CancellationTokenSource>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> EnqueueAsync(JobType type, Dictionary<string, string>? args, CancellationToken ct = default)
        {
            JobStatus status = new JobStatus { Type = type };
            _jobs[status.Id] = status;

            CancellationTokenSource jobCts = new CancellationTokenSource();
            _tokens[status.Id] = jobCts;

            JobWorkItem work = new JobWorkItem
            {
                JobId = status.Id,
                Type = type,
                Args = args,
                Cts = jobCts
            };

            try
            {
                await _queue.Writer.WriteAsync(work, ct);
                return status.Id;
            }
            catch (OperationCanceledException)
            {
                _tokens.TryRemove(status.Id, out _);
                _jobs.TryRemove(status.Id, out _);
                throw;
            }
        }

        public IAsyncEnumerable<JobWorkItem> ReadAllAsync(CancellationToken stoppingToken)
        {
            return _queue.Reader.ReadAllAsync(stoppingToken);
        }

        public bool TryGetStatus(string id, out JobStatus status)
        {
            return _jobs.TryGetValue(id, out status!);
        }

        public IReadOnlyCollection<JobStatus> GetAll(JobType? type = null, JobState? state = null)
        {
            IEnumerable<JobStatus> query = _jobs.Values;
            if (type.HasValue) query = query.Where(s => s.Type == type.Value);
            if (state.HasValue) query = query.Where(s => s.State == state.Value);
            return query.ToArray();
        }

        public bool Cancel(string id)
        {
            if (_tokens.TryGetValue(id, out CancellationTokenSource? cts))
            {
                cts.Cancel();
                return true;
            }
            return false;
        }

        public void Report(string jobId, JobProgress progress)
        {
            if (!_jobs.TryGetValue(jobId, out JobStatus? s)) return;

            if (progress.TotalFound.HasValue) s.TotalFound = progress.TotalFound.Value;
            if (progress.Processed.HasValue) s.Processed = progress.Processed.Value;
            if (progress.Skipped.HasValue) s.Skipped = progress.Skipped.Value;
            if (progress.CurrentItem != null) s.CurrentItem = progress.CurrentItem;
            if (progress.Message != null) s.Message = progress.Message;
            if (progress.Error != null) s.Errors.Add(progress.Error);
        }

        public void MarkRunning(string jobId)
        {
            if (_jobs.TryGetValue(jobId, out JobStatus? s))
            {
                s.State = JobState.Running;
                s.StartedAt = DateTimeOffset.UtcNow;
            }
        }

        public void MarkCompleted(string jobId)
        {
            if (_jobs.TryGetValue(jobId, out JobStatus? s))
            {
                s.State = JobState.Completed;
                s.FinishedAt = DateTimeOffset.UtcNow;
                _tokens.TryRemove(jobId, out _);
            }
        }

        public void MarkFailed(string jobId, string error)
        {
            if (_jobs.TryGetValue(jobId, out JobStatus? s))
            {
                s.State = JobState.Failed;
                s.Errors.Add(error);
                s.FinishedAt = DateTimeOffset.UtcNow;
                _tokens.TryRemove(jobId, out _);
            }
        }

        public void MarkCanceled(string jobId)
        {
            if (_jobs.TryGetValue(jobId, out JobStatus? s))
            {
                s.State = JobState.Canceled;
                s.FinishedAt = DateTimeOffset.UtcNow;
                _tokens.TryRemove(jobId, out _);
            }
        }
    }
}
