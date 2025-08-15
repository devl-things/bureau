using Microsoft.Extensions.Logging;
using Niles.Etl.Abstractions.Extract;
using Niles.Etl.Extract;
using Niles.Etl.Jobs;
using Niles.Etl.Lidl.Models;
using Niles.Models;
using System.Text;

namespace Niles.Etl.Lidl.Services
{
    public sealed class PricesEtlJobHandler : IJobHandler
    {
        private readonly ILogger<PricesEtlJobHandler> _logger;
        private readonly IFileEnumerator _enumerator;
        private readonly IFileNameParser _fileNameParser;
        private readonly CsvExtractorService _csv;
        private readonly RowTransformerService _transform;
        private readonly LoaderService _loader;

        public PricesEtlJobHandler(
            ILogger<PricesEtlJobHandler> logger,
            IFileEnumerator enumerator,
            IFileNameParser fileNameParser,
            CsvExtractorService csv,
            RowTransformerService transform,
            LoaderService loader)
        {
            _logger = logger;
            _enumerator = enumerator;
            _fileNameParser = fileNameParser;
            _csv = csv;
            _transform = transform;
            _loader = loader;
        }

        public JobType Type { get { return JobType.Importer; } }

        public async Task HandleAsync(string jobId, Dictionary<string, string>? args, IJobReporter reporter, CancellationToken cancellationToken = default)
        {
            if (args == null) throw new ArgumentException("Arguments required.");

            string retailerName = Require(args, "retailer");
            string inputFolder = Require(args, "input");
            string encodingName = GetOr(args, "encoding", "windows-1250");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding enc = Encoding.GetEncoding(encodingName);

            List<string> files = _enumerator.EnumerateCsv(inputFolder).ToList();
            reporter.Report(jobId, new JobProgress { TotalFound = files.Count, Message = "Starting ETL..." });

            int done = 0;

            foreach (string file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    (Store store, DateOnly date) meta = _fileNameParser.Parse(retailerName, file);

                    List<LineSnapshot> lines = new List<LineSnapshot>();
                    foreach (CsvRowRaw raw in _csv.ReadRows(file, enc))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        LineSnapshot snap = _transform.Transform(retailerName, meta.store, meta.date, raw);
                        if (!string.IsNullOrWhiteSpace(snap.Product.CanonicalName) && snap.Price.Price > 0m)
                        {
                            lines.Add(snap);
                        }
                    }

                    (int found, int inserted, int upserted) result =
                        await _loader.LoadFileAsync(jobId, file, retailerName, lines, cancellationToken);

                    done++;
                    reporter.Report(jobId, new JobProgress
                    {
                        Processed = done,
                        CurrentItem = Path.GetFileName(file),
                        Message = $"Found {result.found}, inserted {result.inserted}, upserted {result.upserted}"
                    });
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ETL failed for {File}", file);
                    reporter.Report(jobId, new JobProgress { Error = ex.Message, CurrentItem = Path.GetFileName(file) });
                }
            }

            reporter.Report(jobId, new JobProgress { Message = "ETL finished", Processed = done });
        }

        private static string Require(Dictionary<string, string> args, string key)
        {
            if (args.TryGetValue(key, out string? v) && !string.IsNullOrWhiteSpace(v)) return v;
            throw new ArgumentException("Missing argument: " + key);
        }

        private static string GetOr(Dictionary<string, string> args, string key, string fallback)
        {
            if (args.TryGetValue(key, out string? v) && !string.IsNullOrWhiteSpace(v)) return v;
            return fallback;
        }
    }
}
