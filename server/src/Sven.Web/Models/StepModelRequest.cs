using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.PageModels.Connect.SignUp;

namespace Sven
{
    public record StepModelRequest : IVerificationCodeProperties, IPasswordResetProperties
    {
        [FromForm(Name = ViewConstants.PropertyNames.ConfirmPassword)]
        public string? ConfirmPassword { get; set; }
        [FromForm(Name = ViewConstants.PropertyNames.Email)]
        public string? Email { get; set; }
        [FromForm(Name = ViewConstants.PropertyNames.Password)]
        public string? Password { get; set; }
        [FromForm(Name = ViewConstants.PropertyNames.Step)]
        public SignUpStep Step { get; set; } = SignUpStep.EnterEmail;
        [FromForm(Name = ViewConstants.PropertyNames.VerificationCode)]
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
