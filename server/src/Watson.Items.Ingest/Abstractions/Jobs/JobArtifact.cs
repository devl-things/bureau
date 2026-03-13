using Niles.IO;

namespace Niles.Etl.Jobs
{
    public class JobArtifact : IArtifact
    {
        public string Identity { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public ArtifactType Type { get; set; }

        public JobArtifact(string identity, ArtifactType type)
        {
            Identity = identity;
            Type = type;
        }
        public JobArtifact(IArtifact artifact)
        {
            Identity = artifact.Identity;
            Hash = artifact.Hash ?? string.Empty;
            Type = artifact.Type;
        }

        public override string ToString()
        {
            return $"{Identity} ({Hash})";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is JobArtifact ja)
            {
                return Identity == ja.Identity && Hash == ja.Hash && Type == ja.Type;
            }
            return false;
        }
    }
}
