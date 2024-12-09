namespace Bureau.Core
{
    public interface IUserId
    {
        public string Id { get; set; }
    }

    public struct BureauUserId : IUserId
    {
        public BureauUserId(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }
}
