namespace Bureau.Core.Factories
{
    public static class BureauUserIdFactory
    {
        public static IUserId CreateIBureauUserId(string id)
        {
            return new BureauUserId(id);
        }
    }
}
