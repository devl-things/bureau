using Microsoft.Extensions.Options;
using Niles.Etl.Abstractions.Configurations;
using System.Collections.Concurrent;

namespace Niles.Etl.Jobs
{
    public sealed class FileProcessedStore : IProcessedStore
    {
        private readonly string _rootDir;

        // bucketKey -> (key -> _)
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _buckets =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>(StringComparer.OrdinalIgnoreCase);

        // bucketKey -> file path
        private readonly ConcurrentDictionary<string, string> _files =
            new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // bucketKey -> lock object for appends
        private readonly ConcurrentDictionary<string, object> _locks =
            new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public FileProcessedStore(IOptions<DownloaderOptions> options)
        {
            // Reuse your existing TempDirectory as default root; feel free to add a dedicated option later
            string baseDir = string.IsNullOrWhiteSpace(options.Value.TempDirectory)
                ? Path.Combine(AppContext.BaseDirectory, "processed")
                : Path.Combine(options.Value.TempDirectory, "processed");

            Directory.CreateDirectory(baseDir);
            _rootDir = baseDir;
        }

        public bool IsProcessed(JobType jobType, string key, string? scope = null)
        {
            string bucketKey = GetBucketKey(jobType, scope);
            ConcurrentDictionary<string, byte> set = EnsureBucketLoaded(bucketKey, jobType, scope);
            return set.ContainsKey(NormalizeKey(key));
        }

        public bool MarkProcessed(JobType jobType, string key, string? scope = null)
        {
            string normalized = NormalizeKey(key);
            string bucketKey = GetBucketKey(jobType, scope);
            ConcurrentDictionary<string, byte> set = EnsureBucketLoaded(bucketKey, jobType, scope);

            if (set.TryAdd(normalized, 0))
            {
                string file = _files[bucketKey];
                object gate = _locks.GetOrAdd(bucketKey, _ => new object());

                // Append atomically per bucket
                lock (gate)
                {
                    File.AppendAllLines(file, new[] { normalized });
                }
                return true;
            }
            return false;
        }

        private ConcurrentDictionary<string, byte> EnsureBucketLoaded(string bucketKey, JobType jobType, string? scope)
        {
            return _buckets.GetOrAdd(bucketKey, _ =>
            {
                string dir = Path.Combine(_rootDir, jobType.ToString());
                if (!string.IsNullOrWhiteSpace(scope))
                {
                    dir = Path.Combine(dir, Slug(scope!));
                }
                Directory.CreateDirectory(dir);

                string file = Path.Combine(dir, "processed.txt");
                _files[bucketKey] = file;

                ConcurrentDictionary<string, byte> set =
                    new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

                if (File.Exists(file))
                {
                    foreach (string line in File.ReadAllLines(file))
                    {
                        string k = line.Trim();
                        if (k.Length > 0)
                        {
                            set.TryAdd(k, 0);
                        }
                    }
                }

                return set;
            });
        }

        private static string GetBucketKey(JobType jobType, string? scope)
        {
            return scope is null
                ? jobType.ToString()
                : jobType.ToString() + ":" + scope;
        }

        private static string Slug(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }
            return s;
        }

        private static string NormalizeKey(string key)
        {
            // Keep generic: trim + OrdinalIgnoreCase handled by dictionary.
            // For URLs you could additionally trim fragments/query if you want stricter canonicalization.
            return key.Trim();
        }
    }
}
