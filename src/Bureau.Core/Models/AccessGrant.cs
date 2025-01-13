namespace Bureau.Core.Models
{
    public class AccessGrant
    {
        public IReference Resource { get; set; }  // The item or resource being accessed
        public IUserId GrantedTo { get; set; }  // The entity or group with access
        public string PermissionLevel { get; set; } // E.g., "read", "write", "edit"

        public AccessGrant(IReference resource, IUserId grantedTo, string permissionLevel)
        {
            Resource = resource;
            GrantedTo = grantedTo;
            PermissionLevel = permissionLevel;
        }
    }
}
