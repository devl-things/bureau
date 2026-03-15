using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bureau;

namespace Sven.PageModels.Connect.SignUp
{
    public class SignUpStepContext
    {
        private readonly Func<PageResult> _pageFactory;
        private readonly Func<ResultError, PageResult> _pageWithErrorFactory;
        private readonly Func<string, IActionResult> _redirectFactory;

        public StepChallengeRequest StepChallenge { get; init; }
        public StepModelRequest? StepModel { get; init; }
        public SignUpViewModel ViewModel { get; init; }
        public HttpRequest Request { get; init; }

        public SignUpStepContext(
            StepChallengeRequest stepChallenge,
            StepModelRequest? stepModel,
            SignUpViewModel viewModel,
            Func<PageResult> pageFactory,
            Func<ResultError, PageResult> pageWithErrorFactory,
            Func<string, IActionResult> redirectFactory,
            HttpRequest request)
        {
            StepChallenge = stepChallenge;
            StepModel = stepModel;
            ViewModel = viewModel;
            _pageFactory = pageFactory;
            _pageWithErrorFactory = pageWithErrorFactory;
            _redirectFactory = redirectFactory;
            Request = request;
        }

        public PageResult Page() { return _pageFactory(); }
        public PageResult PageWithError(ResultError error) { return _pageWithErrorFactory(error); }
        public IActionResult GoToUrl(string url) { return _redirectFactory(url); }
    }
}
