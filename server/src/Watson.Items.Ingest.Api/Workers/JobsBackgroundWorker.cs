using Bureau.Core;
using Niles.Etl.Jobs;

namespace Niles.Workers
{
    public class JobsBackgroundWorker : BackgroundService
    {
        private readonly ILogger<JobsBackgroundWorker> _logger;
        private readonly IJobManager _jobs;
        private readonly IJobDispatcher _dispatcher;

        public JobsBackgroundWorker(ILogger<JobsBackgroundWorker> logger, IJobManager jobs, IJobDispatcher dispatcher)
        {
            _logger = logger;
            _jobs = jobs;
            _dispatcher = dispatcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (JobWorkItem work in _jobs.ReadAllAsync(stoppingToken))
            {
                _jobs.MarkRunning(work.JobId);

                try
                {
                    Result result = await _dispatcher.DispatchAsync(work, _jobs, work.Cts.Token);

                    if (work.Cts.IsCancellationRequested)
                    {
                        _jobs.MarkCanceled(work.JobId);
                    }
                    else
                    {
                        if (result.IsError)
                        {
                            _logger.LogError("Job {JobId} failed with error: {Error}", work.JobId, result.Error);
                            _jobs.MarkFailed(work.JobId, result.Error.ErrorMessage);
                        }
                        else
                        {
                            _jobs.MarkCompleted(work.JobId);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _jobs.MarkCanceled(work.JobId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Job {JobId} failed.", work.JobId);
                    _jobs.MarkFailed(work.JobId, ex.Message);
                }
            }
        }
    }
}
