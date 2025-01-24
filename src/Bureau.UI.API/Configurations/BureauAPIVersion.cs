using Asp.Versioning;

namespace Bureau.UI.API.Configurations
{
    internal static class BureauAPIVersion
    {
        public static readonly ApiVersion Version1 = new ApiVersion(1);
        public static readonly ApiVersion Version2 = new ApiVersion(2);
        public static readonly ApiVersion Version3 = new ApiVersion(3);
    }
}
