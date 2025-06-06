using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.PageModels.SignUp;

namespace Sven.Models
{
    public record StepModelRequest : IVerificationCodeProperties, IPasswordResetProperties
    {
        [FromForm(Name = AuthConstants.PropertyNames.ConfirmPassword)]
        public string? ConfirmPassword { get; set; }
        [FromForm(Name = AuthConstants.PropertyNames.Email)]
        public string? Email { get; set; }
        [FromForm(Name = AuthConstants.PropertyNames.Password)]
        public string? Password { get; set; }
        [FromForm(Name = AuthConstants.PropertyNames.Step)]
        public SignUpStep Step { get; set; } = SignUpStep.EnterEmail;
        [FromForm(Name = AuthConstants.PropertyNames.VerificationCode)]
        public string? VerificationCode { get; set; }
    }
    public interface IEmailProperty
    {
        public string? Email { get; set; }
    }

    public interface IStepEmailProperties : IEmailProperty
    {
        public SignUpStep Step { get; set; }
    }

    public interface IVerificationCodeProperty
    {
        public string? VerificationCode { get; set; }
    }

    public interface IPasswordResetProperties : IPasswordResetModel, IStepEmailProperties
    {
    }

    public interface IVerificationCodeProperties : IVerificationCodeProperty, IEmailProperty
    {
    }
}
