using Bureau.Core.Models;

namespace Bureau.Core.Factories
{
    public static class BureauReferenceFactory
    {
        public static IReference EmptyReference { get; set; } = new Reference(string.Empty);

        public static bool IsTempReference(IReference reference)
        {
            return IsTempId(reference.Id);
        }
        public static bool IsTempId(string id)
        {
            return id.StartsWith("t-");
        }
        public static string CreateTempId(int id)
        {
            return $"t-{id}";
        }
        public static IReference CreateTempReference(int id)
        {
            return new Reference(CreateTempId(id));
        }
        public static IReference CreateReference(string id)
        {
            return new Reference(id);
        }

        public static bool TryCreateReference(string id, out IReference reference)
        {
            if(Guid.TryParse(id, out _))
            {
                reference = new Reference(id);
                return true;
            }
            reference = default!;
            return false;
        }
    }
}
