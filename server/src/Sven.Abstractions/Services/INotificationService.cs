using Bureau.Core;

namespace Sven.Services
{
    public interface INotificationService<TNotification>
    {
        public Task<Result> NotifyAsync(TNotification notification, CancellationToken cancellationToken);
    }
}
