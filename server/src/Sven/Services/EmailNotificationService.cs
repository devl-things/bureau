using Bureau.Core;
using Sven.Models;

namespace Sven.Services
{
    public class EmailNotificationService : INotificationService<VerificationCodeNotification>
    {
        private readonly ILogger<EmailNotificationService> _logger;
        public EmailNotificationService(ILogger<EmailNotificationService> logger)
        {
            _logger = logger;
        }
        public Task<Result> NotifyAsync(VerificationCodeNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Verification code {Code} sent to {Email}.", notification.Code, notification.Email);
            return Task.FromResult(new Result());
        }
    }
}
