using Bureau;
using Sven.Models;

namespace Sven.Services
{
    public class EmailNotificationService<T> : INotificationService<T>
    {
        private readonly ILogger<EmailNotificationService<T>> _logger;
        public EmailNotificationService(ILogger<EmailNotificationService<T>> logger)
        {
            _logger = logger;
        }
        public Task<Result> NotifyAsync(T notification, CancellationToken cancellationToken)
        {
            switch (notification)
            {
                case UserVerificationCodeNotification vc:
                    _logger.LogWarning("Verification code {Code} sent to {Email}.", vc.Code, vc.Email);
                    break;
                case PasswordResetNotification pr:
                    _logger.LogWarning("Verification code {ResentLink} sent to {Email}.", pr.ResetLink, pr.Email);
                    break;
                default:
                    _logger.LogWarning("Email was sent {Type}", typeof(T));
                    break;
            }

            return Task.FromResult(new Result());
        }
    }
}
