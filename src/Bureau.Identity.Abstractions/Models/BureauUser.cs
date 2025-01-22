using Bureau.Core;

namespace Bureau.Identity.Models
{
    public class BureauUser : IUserId
    {
        public string Id { get; set; }

        public string UserName { get; }

        public string? Email { get; set; }

        //TODO remove
        public BureauUser()
        {
            
        }
        public BureauUser(string id, string userName, string? email)
        {
            Id = id;
            UserName = userName;
            Email = email;
        }
    }
}
