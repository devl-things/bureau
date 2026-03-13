using Niles.Etl.Configurations;
using Niles.Etl.Extract;
using Niles.Etl.Jobs;
using Niles.Etl.Lidl.Models;
using Niles.Etl.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Niles.Etl.Lidl.Services
{
    internal class LidlTransformerService
    {
        private readonly IArtifactRepository _artifactRepository;
        private readonly IFileNameParser _fileNameParser;
        private readonly CsvExtractorService _csv;
        public LidlTransformerService(IArtifactRepository artifactRepository, IFileNameParser fileNameParser, CsvExtractorService csv)
        {
            _artifactRepository = artifactRepository;
            _fileNameParser = fileNameParser;
            _csv = csv;
        }

        public async Task<Result> TransformAsync(JobInfo job, RetailerEtlOptions options, InMemoryFile file, Func<JobArtifact, List<LineSnapshot>, CancellationToken, Task> onTransformationFinishedAsync, CancellationToken cancellationToken)
        {
            JobArtifact artifact = new JobArtifact(file);
            try
            {
                List<LineSnapshot> lines = new List<LineSnapshot>();
                using (file)
                {
                    ArtifactStatus status = await _artifactRepository.GetOrCreateAsync(job.Id, artifact, file.Content.Length, cancellationToken);
                    if (status == ArtifactStatus.Completed)
                    {
                        job.ReportProgress(new JobProgress { Item = artifact, Skipped = 1, Message = "Entry already completed. Skipping." });
                        return false;
                    }

                    (Store store, DateOnly date) meta = _fileNameParser.Parse(file.FullName);

                    file.Content.Position = 0;
                    Encoding enc = Encoding.GetEncoding(string.IsNullOrWhiteSpace(options.Encoding) ? "windows-1250" : options.Encoding);
                    foreach (CsvRowRaw raw in _csv.ReadRows(file.Content, enc))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        LineSnapshot snap = Transform(options.Retailer, meta.store, meta.date, raw);
                        if (!string.IsNullOrWhiteSpace(snap.Product.CanonicalName) && snap.Price.Price > 0m)
                        {
                            lines.Add(snap);
                        }
                    }
                }

                // 4) Load using only lines + hash (no stream dependency)
                ProgressInfo result = await _loader.LoadDataAsync(lines, cancellationToken);

                await _artifactRepository.MarkCompletedAsync(artifact, result, cancellationToken);

                job.ReportProgress(new JobProgress { Item = artifact, Message = result.ToString() });
            }
            catch (OperationCanceledException)
            {
                await _artifactRepository.MarkFailedAsync(artifact, "Operation cancelled", cancellationToken);
            }
            catch (Exception ex)
            {
                await _artifactRepository.MarkFailedAsync(artifact, $"Exception: {ex}", cancellationToken);
                job.ReportProgress(new JobProgress { Item = artifact, Message = $"Error: {ex}" });
            }
            finally
            {
                inflight.Release();
            }
            return artifact;
        }

        private async Task<JobArtifact> TransformAsync(JobInfo job, RetailerEtlOptions options, InMemoryFile inMemoryFile, SemaphoreSlim inflight, CancellationToken cancellationToken)
        {
            JobArtifact artifact = new JobArtifact(inMemoryFile);
            try
            {
                List<LineSnapshot> lines = new List<LineSnapshot>();
                using (inMemoryFile)
                {
                    ArtifactStatus status = await _artifactRepository.GetOrCreateAsync(job.Id, artifact, inMemoryFile.Content.Length, cancellationToken);
                    if (status == ArtifactStatus.Completed)
                    {
                        job.ReportProgress(new JobProgress { Item = artifact, Skipped = 1, Message = "Entry already completed. Skipping." });
                        return artifact;
                    }

                    (Store store, DateOnly date) meta = _fileNameParser.Parse(inMemoryFile.FullName);

                    inMemoryFile.Content.Position = 0;
                    Encoding enc = Encoding.GetEncoding(string.IsNullOrWhiteSpace(options.Encoding) ? "windows-1250" : options.Encoding);
                    foreach (CsvRowRaw raw in _csv.ReadRows(inMemoryFile.Content, enc))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return artifact;
                        }

                        LineSnapshot snap = Transform(options.Retailer, meta.store, meta.date, raw);
                        if (!string.IsNullOrWhiteSpace(snap.Product.CanonicalName) && snap.Price.Price > 0m)
                        {
                            lines.Add(snap);
                        }
                    }
                }

                // 4) Load using only lines + hash (no stream dependency)
                ProgressInfo result = await _loader.LoadDataAsync(lines, cancellationToken);

                await _artifactRepository.MarkCompletedAsync(artifact, result, cancellationToken);

                job.ReportProgress(new JobProgress { Item = artifact, Message = result.ToString() });
            }
            catch (OperationCanceledException)
            {
                await _artifactRepository.MarkFailedAsync(artifact, "Operation cancelled", cancellationToken);
            }
            catch (Exception ex)
            {
                await _artifactRepository.MarkFailedAsync(artifact, $"Exception: {ex}", cancellationToken);
                job.ReportProgress(new JobProgress { Item = artifact, Message = $"Error: {ex}" });
            }
            finally
            {
                inflight.Release();
            }
            return artifact;
        }

        private static readonly CultureInfo Hr = CultureInfo.GetCultureInfo("hr-HR");

        public LineSnapshot Transform(string retailerName, Store storeFromFileName, DateOnly priceDate, CsvRowRaw row)
        {
            Product product = new Product
            {
                CanonicalName = (row.Name ?? string.Empty).Trim(),
                Brand = string.IsNullOrWhiteSpace(row.Brand) ? null : row.Brand!.Trim(),
                Unit = string.IsNullOrWhiteSpace(row.Unit) ? null : row.Unit!.Trim(),
                NetQuantity = TryDec(row.NetQty, out decimal nq) ? nq : null
            };

            ProductRetailerInfo pr = new ProductRetailerInfo
            {
                Code = string.IsNullOrWhiteSpace(row.Code) ? null : row.Code!.Trim(),
                Barcode = string.IsNullOrWhiteSpace(row.Barcode) ? null : CleanBarcode(row.Barcode!)
            };

            PricePoint price = new PricePoint
            {
                Date = priceDate,
                Price = TryDec(row.Price, out decimal p) ? p : 0m,
                UnitPrice = TryDec(row.UnitPrice, out decimal up) ? up : null,
                PromoPrice = TryDec(row.PromoPrice, out decimal promo) ? promo : null,
                Lowest30 = TryDec(row.Lowest30, out decimal l30) ? l30 : null
            };

            LineSnapshot snap = new LineSnapshot
            {
                Retailer = new Retailer { Name = retailerName },
                Store = storeFromFileName,
                Product = product,
                ProductRetailer = pr,
                Price = price
            };
            return snap;
        }

        private static bool TryDec(string? s, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(s)) return false;
            string normalized = s.Trim().Replace(" ", string.Empty).Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private static string CleanBarcode(string raw)
        {
            string s = raw.Trim().Replace(" ", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty);
            if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^[0-9]+(\.[0-9]+)?E\+\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)))
            {
                s = System.Text.RegularExpressions.Regex.Replace(s, @"\D", "", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
            }
            return s;
        }
    }
}
