namespace Sven.PageModels.Account.LinkedIdentities
{
    public class LinkedIdentityModel
    {
        public string Id { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? IconImageUrl { get; set; }
        public string? IconClass { get; set; }
        public bool? Status { get; set; }
        public LinkedIdentityActions AvailableActions { get; set; }
    }

    [Flags]
    public enum LinkedIdentityActions
    {
        None = 0,
        Connect = 1,
        Disconnect = 2,
        Remove = 4
    }
}
