using System.Net;

namespace Bureau.Core.FileSignature
{
    public static class FileExtension
    {
        public static bool Is<T>(this string fileNameOrUrl, out string extractedFileName) where T : IFileSignature, new()
        {
            extractedFileName = ExtractBaseName(fileNameOrUrl);
            if (string.IsNullOrEmpty(extractedFileName)) return false;

            string ext = Path.GetExtension(extractedFileName);
            if (string.IsNullOrEmpty(ext)) return false;

            string extNorm = ext.TrimStart('.');

            foreach (var expected in new T().Extensions)
            {
                if (!string.IsNullOrEmpty(expected) &&
                    string.Equals(extNorm, expected, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ExtractBaseName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            if (Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return WebUtility.UrlDecode(Path.GetFileName(uri.LocalPath));
            }

            return WebUtility.UrlDecode(Path.GetFileName(input));
        }

        public static bool Is<T>(this string fileName) where T : IFileSignature, new()
        {
            return Is<T>(fileName, out _);
        }

        public static bool Is<T>(this byte[] fileBytes) where T : IFileSignature, new()
        {
            if (fileBytes == null || fileBytes.Length == 0) return false;
            return new T().IsMatch(fileBytes);
        }

        public static bool Is<T>(this Stream stream) where T : IFileSignature, new()
        {
            if (stream == null || !stream.CanRead) return false;

            // Read a small prefix (most signatures need ≤ 8 bytes)
            Span<byte> buffer = stackalloc byte[16];
            int read = stream.Read(buffer);
            if (stream.CanSeek) stream.Seek(-read, SeekOrigin.Current); // rewind

            return new T().IsMatch(buffer[..read]);
        }
    }
}
