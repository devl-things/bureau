namespace Niles.IO
{
    public class InMemoryFile : IArtifact, IDisposable
    {
        public string Identity { get; }
        public string Hash { get; }
        public ArtifactType Type => ArtifactType.VirtualFile;

        public string FullName { get; }
        public string Name { get; }
        public MemoryStream Content { get; }


        private bool _disposed;

        public InMemoryFile(string hash, MemoryStream content, string identity, string fullName, string name)
        {
            if (hash == null) throw new ArgumentNullException(nameof(hash));
            if (content == null) throw new ArgumentNullException(nameof(content));

            Hash = hash;
            Content = content;
            Identity = identity;
            FullName = fullName;
            Name = name;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Content.Dispose();
            }
            _disposed = true;
        }
    }
}
