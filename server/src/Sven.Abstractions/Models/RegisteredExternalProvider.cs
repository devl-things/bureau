namespace Sven
{
    /// <summary>
    /// Describes an external OAuth provider registered with Sven at startup.
    /// Used for login page rendering and token vault operations.
    /// </summary>
    /// <param name="ProviderKey">Code name, all lowercase, e.g. "google". Matches AuthConstants.ExternalSchemes.</param>
    /// <param name="DisplayName">Human-readable name, e.g. "Google".</param>
    /// <param name="IconCssClass">Bootstrap Icons CSS class, e.g. "bi-google".</param>
    /// <param name="CssClass">Additional CSS class for the sign-in button, e.g. "btn-google".</param>
    /// <param name="IsSignInEnabled">Whether this provider is shown as a sign-in option on the login page.</param>
    public record RegisteredExternalProvider(
        string ProviderKey,
        string DisplayName,
        string IconCssClass,
        string CssClass,
        bool IsSignInEnabled);
}
