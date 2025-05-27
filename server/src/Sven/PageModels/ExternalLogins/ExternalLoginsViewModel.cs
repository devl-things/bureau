namespace Sven.PageModels.ExternalLogins
{
    public class ExternalLoginsViewModel : IExternalLoginProperty
    {
        public string PrefixText { get; set; }
        public List<ExternalLoginModel> ExternalLogins { get; init; }
    }
}
