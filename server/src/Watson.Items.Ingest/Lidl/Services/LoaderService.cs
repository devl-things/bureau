using Bureau.Core;
using Niles.Data;
using Niles.Etl.Jobs;
using Niles.Etl.Models;
using Niles.Etl.Transform;

namespace Niles.Etl.Lidl.Services
{
    public sealed class LoaderService
    {
        private readonly IChecksumService _checksum;
        private readonly IArtifactRepository _files;
        private readonly IPriceRepository _repo;
        private readonly IFileManager _archiver;

        public LoaderService(IChecksumService checksum, IArtifactRepository files, IPriceRepository repo, IFileManager archiver)
        {
            _checksum = checksum;
            _files = files;
            _repo = repo;
            _archiver = archiver;
        }


        public async Task<ProgressInfo> LoadDataAsync(List<LineSnapshot> data, CancellationToken cancellationToken = default)
        {
            ProgressInfo result = new JobProgress();
            return result;
        }

        public async Task<(int found, int inserted, int upserted)> LoadFileAsync(
            string jobId,
            string filePath,
            string retailerName,
            IEnumerable<LineSnapshot> lines,
            CancellationToken ct)
        {
            string sha = _checksum.ComputeSha256(filePath);
            ArtifactStatus? existing = await _files.GetStatusByHashAsync(sha, ct);
            if (existing == ArtifactStatus.Completed)
            {
                // Already processed successfully — skip silently.
                return (0, 0, 0);
            }

            if (existing == null)
            {
                FileInfo fi = new FileInfo(filePath);
                await _files.CreatePendingAsync(jobId, fi.Name, filePath, sha, fi.Length, ct);
            }

            await _files.MarkInProgressAsync(sha, ct);

            int found = 0;
            int inserted = 0;
            int upserted = 0;

            await using (IUnitOfWork uow = await _repo.BeginAsync(ct))
            {
                try
                {
                    Guid retailerId = await _repo.UpsertRetailerAsync(retailerName, ct);

                    // local caches to minimize db calls inside a single file
                    Dictionary<string, Guid> stores = new Dictionary<string, Guid>(StringComparer.Ordinal);
                    Dictionary<string, Guid> products = new Dictionary<string, Guid>(StringComparer.Ordinal);

                    foreach (LineSnapshot line in lines)
                    {
                        ct.ThrowIfCancellationRequested();
                        found++;

                        Guid storeId;
                        if (!stores.TryGetValue(line.Store.Name, out storeId))
                        {
                            storeId = await _repo.UpsertStoreAsync(retailerId, line.Store.Name, line.Store.Address, line.Store.City, line.Store.PostalCode, ct);
                            stores[line.Store.Name] = storeId;
                        }

                        string productKey = line.Product.CanonicalName;
                        Guid productId;
                        if (!products.TryGetValue(productKey, out productId))
                        {
                            productId = await _repo.UpsertProductAsync(
                                line.Product.CanonicalName,
                                line.Product.Brand,
                                line.Product.Unit,
                                line.Product.NetQuantity,
                                ct);
                            products[productKey] = productId;
                        }

                        await _repo.UpsertProductRetailerAsync(productId, retailerId, line.ProductRetailer.Code, line.ProductRetailer.Barcode, ct);

                        bool insertedPrice = await _repo.UpsertPriceAsync(
                            productId,
                            storeId,
                            line.Price.Date,
                            line.Price.Price,
                            line.Price.UnitPrice,
                            line.Price.PromoPrice,
                            line.Price.Lowest30,
                            ct);

                        if (insertedPrice) inserted++; else upserted++;
                    }

                    await uow.CommitAsync(ct);

                    await _files.MarkCompletedAsync(sha, found, inserted, upserted, ct);
                    _archiver.MoveToArchive(filePath);

                    return (found, inserted, upserted);
                }
                catch (OperationCanceledException)
                {
                    await uow.RollbackAsync(ct);
                    await _files.MarkFailedAsync(sha, "Canceled", ct);
                    _archiver.MoveToError(filePath);
                    throw;
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync(ct);
                    await _files.MarkFailedAsync(sha, ex.Message, ct);
                    _archiver.MoveToError(filePath);
                    throw;
                }
            }
        }
    }
}
