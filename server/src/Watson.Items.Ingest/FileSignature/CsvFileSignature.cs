namespace Bureau.Core.FileSignature
{
    public sealed class CsvFileSignature : IFileSignature
    {
        private const int MaxProbeBytes = 8192;
        private static readonly byte[] Utf8Bom = [0xEF, 0xBB, 0xBF];
        private static readonly byte[] DelimiterCandidates = new byte[] { (byte)',', (byte)';', (byte)'\t', (byte)'|' };

        public IEnumerable<string> Extensions
        {
            get { return new[] { "csv" }; }
        }

        public bool IsMatch(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length == 0) return false;

            // Quick reject: ZIP/XLSX etc.
            if (new ZipFileSignature().IsMatch(bytes)) return false;

            ReadOnlySpan<byte> probe = bytes.Slice(0, Math.Min(MaxProbeBytes, bytes.Length));

            if (ContainsNullByte(probe)) return false;
            if (!LooksLikeText(probe)) return false;

            int start = StartsWithUtf8Bom(probe) ? Utf8Bom.Length : 0;
            ReadOnlySpan<byte> span = probe.Slice(start);

            if (!ContainsNewline(span)) return false;

            // Delimiter detection over first N lines
            byte detected;
            if (!TryDetectDelimiter(span, 10, out detected)) return false;

            return true;
        }

        private static bool ContainsNullByte(ReadOnlySpan<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == 0x00) return true;
            }
            return false;
        }

        private static bool LooksLikeText(ReadOnlySpan<byte> span)
        {
            // Allow CR/LF/TAB; limit other control chars
            int controlCount = 0;
            int limit = Math.Min(span.Length, MaxProbeBytes);
            for (int i = 0; i < limit; i++)
            {
                byte b = span[i];
                if (b < 0x20 && b != 0x09 && b != 0x0A && b != 0x0D)
                {
                    controlCount++;
                    if (controlCount > 4) return false;
                }
            }
            return true;
        }

        private static bool StartsWithUtf8Bom(ReadOnlySpan<byte> span)
        {
            if (span.Length < 3) return false;
            return span[0] == Utf8Bom[0] && span[1] == Utf8Bom[1] && span[2] == Utf8Bom[2];
        }

        private static bool ContainsNewline(ReadOnlySpan<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                byte b = span[i];
                if (b == 0x0A || b == 0x0D) return true;
            }
            return false;
        }

        private static bool TryDetectDelimiter(ReadOnlySpan<byte> span, int maxLines, out byte delimiter)
        {
            // For each candidate, keep simple counts; avoid storing spans in fields
            int[] linesWithDelims = new int[DelimiterCandidates.Length];
            // Track a small histogram of field counts for stability check (mode + off-by-one)
            // We only need a handful of entries, so store last 16 counts per candidate.
            int[,] recentCounts = new int[DelimiterCandidates.Length, 16];
            int[] countUsed = new int[DelimiterCandidates.Length];

            int index = 0;
            int linesChecked = 0;

            while (index < span.Length && linesChecked < maxLines)
            {
                int lineStart = index;
                int lineLen = ReadLine(span, ref index); // returns length of the line (including newline chars)

                if (lineLen <= 0) continue;
                linesChecked++;

                ReadOnlySpan<byte> line = span.Slice(lineStart, lineLen);

                for (int c = 0; c < DelimiterCandidates.Length; c++)
                {
                    byte cand = DelimiterCandidates[c];
                    int fields = CountCsvFields(line, cand);
                    if (fields >= 2)
                    {
                        linesWithDelims[c]++;
                        int used = countUsed[c];
                        if (used < 16)
                        {
                            recentCounts[c, used] = fields;
                            countUsed[c] = used + 1;
                        }
                    }
                }
            }

            int bestIdx = -1;
            int bestScore = -1;

            for (int c = 0; c < DelimiterCandidates.Length; c++)
            {
                if (linesWithDelims[c] < 2) continue;
                if (!HasStableFieldCount(recentCounts, countUsed, c)) continue;

                // Prefer the candidate that matched the most lines
                if (linesWithDelims[c] > bestScore)
                {
                    bestScore = linesWithDelims[c];
                    bestIdx = c;
                }
            }

            if (bestIdx >= 0)
            {
                delimiter = DelimiterCandidates[bestIdx];
                return true;
            }

            delimiter = (byte)',';
            return false;
        }

        private static int ReadLine(ReadOnlySpan<byte> span, ref int index)
        {
            int start = index;
            while (index < span.Length)
            {
                byte b = span[index++];
                if (b == 0x0A) break; // \n
                if (b == 0x0D)        // \r or \r\n
                {
                    if (index < span.Length && span[index] == 0x0A) index++;
                    break;
                }
            }
            return index - start;
        }

        private static int CountCsvFields(ReadOnlySpan<byte> line, byte delimiter)
        {
            // RFC4180-ish: quotes and escaped quotes ("")
            bool inQuotes = false;
            int fields = 1;
            int i = 0;

            while (i < line.Length)
            {
                byte b = line[i];

                if (b == (byte)'"')
                {
                    if (inQuotes)
                    {
                        if (i + 1 < line.Length && line[i + 1] == (byte)'"')
                        {
                            i += 2; // skip escaped quote
                            continue;
                        }
                        inQuotes = false;
                        i++;
                        continue;
                    }
                    else
                    {
                        inQuotes = true;
                        i++;
                        continue;
                    }
                }

                if (b == delimiter && !inQuotes)
                {
                    fields++;
                }

                i++;
            }

            return fields;
        }

        private static bool HasStableFieldCount(int[,] recentCounts, int[] countUsed, int candidateIndex)
        {
            int used = countUsed[candidateIndex];
            if (used < 2) return false;

            // Compute mode (simple O(n^2) since used <= 16)
            int mode = 0;
            int modeCount = -1;
            for (int i = 0; i < used; i++)
            {
                int val = recentCounts[candidateIndex, i];
                int cnt = 0;
                for (int j = 0; j < used; j++)
                {
                    if (recentCounts[candidateIndex, j] == val) cnt++;
                }
                if (cnt > modeCount)
                {
                    modeCount = cnt;
                    mode = val;
                }
            }

            int offByOne = 0;
            for (int i = 0; i < used; i++)
            {
                int diff = recentCounts[candidateIndex, i] - mode;
                if (diff < 0) diff = -diff;
                if (diff > 1) return false;
                if (diff == 1) offByOne++;
            }

            // allow a little wobble
            return offByOne <= Math.Max(1, used / 4);
        }
    }
}
