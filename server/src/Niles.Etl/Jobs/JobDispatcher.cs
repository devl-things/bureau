using Microsoft.Extensions.DependencyInjection;

namespace Niles.Etl.Jobs
{
    public sealed class JobDispatcher : IJobDispatcher
    {
        private readonly IServiceProvider _sp;
        public JobDispatcher(IServiceProvider sp) { _sp = sp; }

        public async Task DispatchAsync(JobWorkItem work, IJobReporter reporter, CancellationToken cancellationToken = default)
        {
            using IServiceScope scope = _sp.CreateScope();
            IJobHandler handler = scope.ServiceProvider.GetRequiredKeyedService<IJobHandler>(work.Type);
            await handler.HandleAsync(work.JobId, work.Args, reporter, cancellationToken);
        }
    }
}
