using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.PageModels.Connect.SignUp;

namespace Sven
{
    public record StepChallengeRequest
    {
        private string _stepShort = string.Empty;
        [FromQuery(Name = ViewConstants.PropertyNames.Step)]
        public string StepShort
        {
            get { return _stepShort; }
            set
            {
                _stepShort = value;
                Step = SignUpStepExtensions.ToSignUpStep(_stepShort);
            }
        }

        public SignUpStep Step { get; protected set; }
        [FromQuery(Name = AuthConstants.PropertyNames.Challenge)]
        public string? Challenge { get; set; }

        public StepChallengeRequest(SignUpStep step)
        {
            _stepShort = step.ToCode();
            Step = step;
        }
        public StepChallengeRequest()
        {

        }

    }
}
