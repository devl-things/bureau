using Niles.Etl.Lidl.Models;
using System.Text;

namespace Niles.Etl.Lidl.Services
{
    public sealed class CsvExtractorService
    {
        public IEnumerable<CsvRowRaw> ReadRows(string filePath, Encoding encoding)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(fs, encoding, detectEncodingFromByteOrderMarks: true))
            {
                foreach (CsvRowRaw row in ReadRowsCore(sr))
                {
                    yield return row;
                }
            }
        }

        /// <summary>
        /// Reads CSV rows from an already-open stream. The stream is left open.
        /// </summary>
        public IEnumerable<CsvRowRaw> ReadRows(Stream stream, Encoding encoding)
        {
            using (StreamReader sr = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
            {
                foreach (CsvRowRaw row in ReadRowsCore(sr))
                {
                    yield return row;
                }
            }
        }

        private static IEnumerable<CsvRowRaw> ReadRowsCore(TextReader reader)
        {
            string? header = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(header))
            {
                yield break;
            }

            string[] headers = SplitCsv(header);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] fields = SplitCsv(line);
                CsvRowRaw row = Map(headers, fields);
                if (!string.IsNullOrWhiteSpace(row.Name))
                {
                    yield return row;
                }
            }
        }

        private static string[] SplitCsv(string line)
        {
            List<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            result.Add(sb.ToString());
            return result.ToArray();
        }

        private static CsvRowRaw Map(string[] headers, string[] fields)
        {
            CsvRowRaw r = new CsvRowRaw();

            for (int i = 0; i < headers.Length; i++)
            {
                string h = headers[i].Trim();
                string v = i < fields.Length ? fields[i] : string.Empty;

                if (h.Equals("NAZIV", StringComparison.OrdinalIgnoreCase)) r.Name = v;
                else if (h.Equals("MARKA", StringComparison.OrdinalIgnoreCase)) r.Brand = v;
                else if (h.StartsWith("JEDINICA_MJERE", StringComparison.OrdinalIgnoreCase)) r.Unit = v;
                else if (h.StartsWith("NETO_KOLI", StringComparison.OrdinalIgnoreCase)) r.NetQty = v;
                else if (h.Equals("ŠIFRA", StringComparison.OrdinalIgnoreCase) || h.Equals("SIFRA", StringComparison.OrdinalIgnoreCase)) r.Code = v;
                else if (h.Equals("BARKOD", StringComparison.OrdinalIgnoreCase)) r.Barcode = v;
                else if (h.StartsWith("MALOPRODAJNA_CIJENA", StringComparison.OrdinalIgnoreCase)) r.Price = v;
                else if (h.StartsWith("CIJENA_ZA_JEDINICU_MJERE", StringComparison.OrdinalIgnoreCase)) r.UnitPrice = v;
                else if (h.StartsWith("MPC_ZA_VRIJEME_POSEBNOG_OBLIKA_PRODAJE", StringComparison.OrdinalIgnoreCase)) r.PromoPrice = v;
                else if (h.StartsWith("NAJNIZA_CIJENA_U_POSLJ.", StringComparison.OrdinalIgnoreCase)) r.Lowest30 = v;
            }

            return r;
        }
    }
}
