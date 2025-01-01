using Bureau.Core;
using Bureau.Core.Comparers;
using Bureau.Core.Extensions;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Data.Postgres.Contexts;
using Bureau.Data.Postgres.Handlers;
using Bureau.Data.Postgres.Mappers;
using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Linq;
using System.Text;
using dbModels = Bureau.Data.Postgres.Models;

namespace Bureau.Data.Postgres.Repositories
{
    internal class RecordRepository : IRepository
    {
        private readonly BureauContext _dbContext;
        private readonly string? _connectionString;
        //TODO put in options
        private readonly int BATCH_SIZE = 1000;
        public RecordRepository(BureauContext dbContext)
        {
            _dbContext = dbContext;
            _connectionString = _dbContext.Database.GetConnectionString();
        }
        //TODO [refactor] FetchRecords methods
        public async Task<Result<AggregateModel>> FetchRecordsAsync(IdSearchRequest idSearchRequest, CancellationToken cancellationToken)
        {
            if (idSearchRequest.FilterReferenceId is null) 
            {
                return "Reference Id is required";
            }
            AggregateModel aggregateModel = default!;

            // Variables declared outside to ensure they exist throughout the method
            List<Postgres.Models.Edge> filteredEdges;
            HashSet<Guid> relatedRecordIds = new();
            Dictionary<Guid, Record> records;

            // Step 1: Create the main connection for filtering edges and fetching records
            await using (NpgsqlConnection mainConnection = new NpgsqlConnection(_connectionString))
            {
                await mainConnection.OpenAsync(cancellationToken);

                // Step 2: Filter Edges based on FilterReferenceId and FilterRequestType
                filteredEdges = await FilterEdgesAsync(mainConnection, idSearchRequest, cancellationToken);

                // Step 3: Gather all related Record IDs based on SelectReferences
                foreach (Postgres.Models.Edge edge in filteredEdges)
                {
                    if (idSearchRequest.SelectReferences.HasFlag(EdgeRequestType.Edge))
                        relatedRecordIds.Add(edge.Id);

                    if (idSearchRequest.SelectReferences.HasFlag(EdgeRequestType.SourceNode))
                        relatedRecordIds.Add(edge.SourceNodeId);

                    if (idSearchRequest.SelectReferences.HasFlag(EdgeRequestType.TargetNode))
                        relatedRecordIds.Add(edge.TargetNodeId);

                    if (idSearchRequest.SelectReferences.HasFlag(EdgeRequestType.RootNode))
                        relatedRecordIds.Add(edge.RootNodeId);
                }

                // Set MainReference based on FilterReferenceId
                if (relatedRecordIds.Contains(Guid.Parse(idSearchRequest.FilterReferenceId.Id)))
                {
                    aggregateModel = new AggregateModel()
                    {
                        MainReference = BureauReferenceFactory.CreateReference(idSearchRequest.FilterReferenceId.Id)
                    };
                }
                else
                {
                    return "Reference ID not found in the database";
                }

                // Step 4: Fetch related Records
                records = await FetchRecordsByIdsAsync(mainConnection, relatedRecordIds, cancellationToken);
            }

            

            // Step 5: Populate additional entities based on SelectRecordTypes using separate connections
            var tasks = new List<Task>();

            if (idSearchRequest.SelectRecordTypes.HasFlag(RecordRequestType.Edges))
            {
                tasks.Add(Task.Run(() =>
                {
                    filteredEdges.ForEach(x => x.Record = records[x.Id]);
                    IEnumerable<Core.Models.Edge> r = filteredEdges.Select(x => x.ToModel());
                    aggregateModel.Edges = new HashSet<Core.Models.Edge>(r, new ReferenceComparer());
                }, cancellationToken));
            }

            if (idSearchRequest.SelectRecordTypes.HasFlag(RecordRequestType.TermEntries))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await using (var termConnection = new NpgsqlConnection(_connectionString))
                    {
                        await termConnection.OpenAsync(cancellationToken);

                        var termEntries = await FetchTermEntriesByIdsAsync(termConnection, relatedRecordIds, records, cancellationToken);
                        IEnumerable<Core.Models.TermEntry> r = termEntries.Select(x => x.ToModel());
                        aggregateModel.TermEntries = new HashSet<Core.Models.TermEntry>(r, new ReferenceComparer());
                    }
                }, cancellationToken));
            }

            if (idSearchRequest.SelectRecordTypes.HasFlag(RecordRequestType.FlexRecords))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await using (var flexConnection = new NpgsqlConnection(_connectionString))
                    {
                        await flexConnection.OpenAsync(cancellationToken);

                        HashSet<FlexibleRecord> flexRecords = await FetchFlexibleRecordsByIdsAsync(flexConnection, relatedRecordIds, records, cancellationToken);
                        IEnumerable<FlexRecord> r = flexRecords.Select(flexRecord => flexRecord.ToModel());
                        aggregateModel.FlexRecords = new HashSet<FlexRecord>(r, new ReferenceComparer());
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            return aggregateModel;
        }


        public async Task<Result<BaseAggregateModel>> FetchRecordsAsync(EdgeTypeSearchRequest edgeTypeSearchRequest, CancellationToken cancellationToken)
        {
            BaseAggregateModel aggregateModel = new BaseAggregateModel();

            // Variables declared outside to ensure they exist throughout the method
            List<Postgres.Models.Edge> filteredEdges;
            HashSet<Guid> relatedRecordIds = new();
            Dictionary<Guid, Record> records;

            // Step 1: Create the main connection for filtering edges and fetching records
            await using (NpgsqlConnection mainConnection = new NpgsqlConnection(_connectionString))
            {
                await mainConnection.OpenAsync(cancellationToken);

                // Step 2: Filter Edges based on FilterReferenceId and FilterRequestType
                filteredEdges = await FilterEdgesAsync(mainConnection, edgeTypeSearchRequest, cancellationToken);

                // Step 3: Gather all related Record IDs based on SelectReferences
                foreach (Postgres.Models.Edge edge in filteredEdges)
                {
                    if (edgeTypeSearchRequest.SelectReferences.HasFlag(EdgeRequestType.Edge))
                        relatedRecordIds.Add(edge.Id);

                    if (edgeTypeSearchRequest.SelectReferences.HasFlag(EdgeRequestType.SourceNode))
                        relatedRecordIds.Add(edge.SourceNodeId);

                    if (edgeTypeSearchRequest.SelectReferences.HasFlag(EdgeRequestType.TargetNode))
                        relatedRecordIds.Add(edge.TargetNodeId);

                    if (edgeTypeSearchRequest.SelectReferences.HasFlag(EdgeRequestType.RootNode))
                        relatedRecordIds.Add(edge.RootNodeId);
                }

                // Step 4: Fetch related Records
                records = await FetchRecordsByIdsAsync(mainConnection, relatedRecordIds, cancellationToken);
            }

            // Step 5: Populate additional entities based on SelectRecordTypes using separate connections
            var tasks = new List<Task>();

            if (edgeTypeSearchRequest.SelectRecordTypes.HasFlag(RecordRequestType.Edges))
            {
                tasks.Add(Task.Run(() =>
                {
                    filteredEdges.ForEach(x => x.Record = records[x.Id]);
                    IEnumerable<Core.Models.Edge> r = filteredEdges.Select(x => x.ToModel());
                    aggregateModel.Edges = new HashSet<Core.Models.Edge>(r, new ReferenceComparer());
                }, cancellationToken));
            }

            if (edgeTypeSearchRequest.SelectRecordTypes.HasFlag(RecordRequestType.TermEntries))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await using (var termConnection = new NpgsqlConnection(_connectionString))
                    {
                        await termConnection.OpenAsync(cancellationToken);

                        var termEntries = await FetchTermEntriesByIdsAsync(termConnection, relatedRecordIds, records, cancellationToken);
                        IEnumerable<Core.Models.TermEntry> r = termEntries.Select(x => x.ToModel());
                        aggregateModel.TermEntries = new HashSet<Core.Models.TermEntry>(r, new ReferenceComparer());
                    }
                }, cancellationToken));
            }

            if (edgeTypeSearchRequest.SelectRecordTypes.HasFlag(RecordRequestType.FlexRecords))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await using (var flexConnection = new NpgsqlConnection(_connectionString))
                    {
                        await flexConnection.OpenAsync(cancellationToken);

                        HashSet<FlexibleRecord> flexRecords = await FetchFlexibleRecordsByIdsAsync(flexConnection, relatedRecordIds, records, cancellationToken);
                        IEnumerable<FlexRecord> r = flexRecords.Select(flexRecord => flexRecord.ToModel());
                        aggregateModel.FlexRecords = new HashSet<FlexRecord>(r, new ReferenceComparer());
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            return aggregateModel;
        }

        private async Task<List<Postgres.Models.Edge>> FilterEdgesAsync(
            NpgsqlConnection connection,
            IdSearchRequest idSearchRequest,
            CancellationToken cancellationToken)
        {
            var edges = new List<Postgres.Models.Edge>();

            // Build dynamic WHERE clause based on FilterRequestType flags
            var filterConditions = new List<string>();

            if (idSearchRequest.FilterRequestType.HasFlag(EdgeRequestType.Edge))
                filterConditions.Add("\"Id\" = @ReferenceId");

            if (idSearchRequest.FilterRequestType.HasFlag(EdgeRequestType.SourceNode))
                filterConditions.Add("\"SourceNodeId\" = @ReferenceId");

            if (idSearchRequest.FilterRequestType.HasFlag(EdgeRequestType.TargetNode))
                filterConditions.Add("\"TargetNodeId\" = @ReferenceId");

            if (idSearchRequest.FilterRequestType.HasFlag(EdgeRequestType.RootNode))
                filterConditions.Add("\"RootNodeId\" = @ReferenceId");

            // Combine conditions into a single WHERE clause
            var whereClause = string.Join(" OR ", filterConditions);

            string query = string.Format(@"
        SELECT 
            ""Id"", 
            ""SourceNodeId"", 
            ""TargetNodeId"", 
            ""EdgeType"", 
            ""Active"", 
            ""ParentNodeId"", 
            ""RootNodeId"", 
            ""Order""
        FROM public.""Edges""
        WHERE {0};", whereClause);

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ReferenceId", NpgsqlTypes.NpgsqlDbType.Uuid, Guid.Parse(idSearchRequest.FilterReferenceId.Id));

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        edges.Add(new Postgres.Models.Edge
                        {
                            Id = reader.GetGuid(0),
                            SourceNodeId = reader.GetGuid(1),
                            TargetNodeId = reader.GetGuid(2),
                            EdgeType = reader.GetInt32(3),
                            Active = reader.GetBoolean(4),
                            ParentNodeId = reader.IsDBNull(5) ? (Guid?)null : reader.GetGuid(5),
                            RootNodeId = reader.GetGuid(6),
                            Order = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7)
                        });
                    }
                }
            }

            return edges;
        }

        private async Task<List<Postgres.Models.Edge>> FilterEdgesAsync(
            NpgsqlConnection connection,
            EdgeTypeSearchRequest edgeTypeSearchRequest,
            CancellationToken cancellationToken)
        {
            List<Postgres.Models.Edge> edges = new List<Postgres.Models.Edge>();
            
            string whereClause = string.Empty;
            if (edgeTypeSearchRequest.Active.HasValue) 
            {
                whereClause = " AND \"Active\" = @EdgeActive";
            }
            //TODO [first] filter by FilterRequestType
            string query = string.Format(@"
        SELECT 
            ""Id"", 
            ""SourceNodeId"", 
            ""TargetNodeId"", 
            ""EdgeType"", 
            ""Active"", 
            ""ParentNodeId"", 
            ""RootNodeId"", 
            ""Order""
        FROM public.""Edges""
        WHERE ""EdgeType"" = @EdgeType
            {0}
        UNION ALL
        SELECT 
            ""Id"", 
            ""SourceNodeId"", 
            ""TargetNodeId"", 
            ""EdgeType"", 
            ""Active"", 
            ""ParentNodeId"", 
            ""RootNodeId"", 
            ""Order""
        FROM public.""Edges""
        WHERE ""RootNodeId"" in (SELECT ""Id"" FROM public.""Edges"" WHERE ""EdgeType"" = @EdgeType {0});", whereClause);

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EdgeType", NpgsqlTypes.NpgsqlDbType.Integer, edgeTypeSearchRequest.EdgeType);
                if (edgeTypeSearchRequest.Active.HasValue)
                {
                    command.Parameters.AddWithValue("@EdgeActive", NpgsqlTypes.NpgsqlDbType.Boolean, edgeTypeSearchRequest.Active.Value);
                }

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        edges.Add(new Postgres.Models.Edge
                        {
                            Id = reader.GetGuid(0),
                            SourceNodeId = reader.GetGuid(1),
                            TargetNodeId = reader.GetGuid(2),
                            EdgeType = reader.GetInt32(3),
                            Active = reader.GetBoolean(4),
                            ParentNodeId = reader.IsDBNull(5) ? (Guid?)null : reader.GetGuid(5),
                            RootNodeId = reader.GetGuid(6),
                            Order = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7)
                        });
                    }
                }
            }

            return edges;
        }

        private async Task<Dictionary<Guid, Record>> FetchRecordsByIdsAsync(
            NpgsqlConnection connection,
            HashSet<Guid> ids,
            CancellationToken cancellationToken)
        {
            var records = new Dictionary<Guid, Record>();

            if (ids.Count == 0)
                return records;

            string query = @"
        SELECT 
            ""Id"", 
            ""Status"", 
            ""CreatedAt"", 
            ""CreatedBy"", 
            ""UpdatedAt"", 
            ""UpdatedBy"", 
            ""ProviderName"", 
            ""ExternalId""
        FROM public.""Records""
        WHERE ""Id"" = ANY(@Ids);";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Uuid, ids.ToArray());

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var record = new Record
                        {
                            Id = reader.GetGuid(0),
                            Status = reader.GetInt32(1),
                            CreatedAt = reader.GetDateTime(2),
                            CreatedBy = reader.GetString(3),
                            UpdatedAt = reader.GetDateTime(4),
                            UpdatedBy = reader.GetString(5),
                            ProviderName = reader.GetString(6),
                            ExternalId = reader.IsDBNull(7) ? null : reader.GetString(7)
                        };

                        records[record.Id] = record;
                    }
                }
            }

            return records;
        }

        private async Task<HashSet<Postgres.Models.TermEntry>> FetchTermEntriesByIdsAsync(
            NpgsqlConnection connection,
            HashSet<Guid> ids,
            Dictionary<Guid, Record> records,
            CancellationToken cancellationToken)
        {
            var termEntries = new HashSet<Postgres.Models.TermEntry>();

            string query = @"
        SELECT 
            ""Id"", 
            ""Title"", 
            ""Label""
        FROM public.""TermEntries""
        WHERE ""Id"" = ANY(@Ids);";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Uuid, ids.ToArray());

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var termEntry = new Postgres.Models.TermEntry
                        {
                            Id = reader.GetGuid(0),
                            Title = reader.GetString(1),
                            Label = reader.GetString(2),
                            Record = records.TryGetValue(reader.GetGuid(0), out var record) ? record : null
                        };

                        termEntries.Add(termEntry);
                    }
                }
            }

            return termEntries;
        }

        private async Task<HashSet<FlexibleRecord>> FetchFlexibleRecordsByIdsAsync(
            NpgsqlConnection connection,
            HashSet<Guid> ids,
            Dictionary<Guid, Record> records,
            CancellationToken cancellationToken)
        {
            var flexibleRecords = new HashSet<FlexibleRecord>();

            string query = @"
        SELECT 
            ""Id"", 
            ""DataType"", 
            ""Data""
        FROM public.""FlexibleRecords""
        WHERE ""Id"" = ANY(@Ids);";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Uuid, ids.ToArray());

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var flexibleRecord = new FlexibleRecord
                        {
                            Id = reader.GetGuid(0),
                            DataType = reader.GetString(1),
                            Data = reader.GetString(2),
                            Record = records.TryGetValue(reader.GetGuid(0), out var record) ? record : null
                        };

                        flexibleRecords.Add(flexibleRecord);
                    }
                }
            }

            return flexibleRecords;
        }


        public async Task<Result<Dictionary<string, Core.Models.TermEntry>>> FetchTermRecordsAsync(TermSearchRequest termSearchRequest, CancellationToken cancellationToken)
        {
            try
            {
                if (termSearchRequest.Terms == null || termSearchRequest.Terms.Count <= 0)
                {
                    return new Dictionary<string, Core.Models.TermEntry>(0);
                }
                Dictionary<string, Core.Models.TermEntry> matchingEntries;
                if (termSearchRequest.RequestType == TermRequestType.Label)
                {
                    matchingEntries = await _dbContext.TermEntries
                        .Include(x => x.Record)
                        .Where(x => termSearchRequest.Terms.Contains(x.Label))
                        .ToDictionaryAsync(k => k.Label, v => v.ToModel(), cancellationToken);
                }
                else
                {
                    matchingEntries = await _dbContext.TermEntries
                        .Include(x => x.Record)
                        .Where(x => termSearchRequest.Terms.Contains(x.Title))
                        .ToDictionaryAsync(k => k.Title, v => v.ToModel(), cancellationToken);
                }
                return matchingEntries;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public async Task<Result<IReference>> InsertAggregateAsync(AggregateModel insertRequest, CancellationToken cancellationToken)
        {
            AggregateModelHandler handler = new AggregateModelHandler(insertRequest);
            Result result = handler.HandleAggregate();
            if (result.IsError)
            {
                return result.Error;
            }
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    using (NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken))
                    {
                        try
                        { 
                            await BulkInsertRecordsAsync(connection, transaction, handler.NewRecords, cancellationToken);
                            _dbContext.Database.SetDbConnection(connection);
                            await _dbContext.Database.UseTransactionAsync(transaction, cancellationToken);
                            await _dbContext.Records
                                .Where(x => handler.UpdateRecords.Select(x => x.Id).Contains(x.Id.ToString()))
                                .ExecuteUpdateAsync(te => te.SetProperty(e => e.UpdatedAt, e => e.UpdatedAt));
                            await BulkInsertTermEntriesAsync(connection, transaction, handler.NewTermEntries, cancellationToken);
                            await BulkInsertEdgesAsync(connection, transaction, handler.NewEdgeRecords, cancellationToken);
                            await BulkUpdateEdgesAsync(connection, transaction, handler.UpdateEdgeRecords, cancellationToken);
                            await BulkInsertFlexibleRecordsAsync(connection, transaction, handler.NewFlexibleRecords, cancellationToken);
                            await BulkUpdateFlexibleRecordsAsync(connection, transaction, handler.UpdateFlexibleRecords, cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            // Rollback transaction in case of an error
                            await transaction.RollbackAsync(cancellationToken);
                            return ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO logging
                return ex.Message;
            }

            return new Result<IReference>(handler.MainReference);
        }

        public async Task<Result<IReference>> UpdateAggregateAsync(ExtendedAggregateModel updateRequest, CancellationToken cancellationToken)
        {
            ExtendedAggregateModelHandler handler = new ExtendedAggregateModelHandler(updateRequest);
            Result result = handler.HandleAggregate();
            if (result.IsError)
            {
                return result.Error;
            }
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    using (NpgsqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken))
                    {
                        try
                        {
                            await BulkInsertRecordsAsync(connection, transaction, handler.NewRecords, cancellationToken);
                            _dbContext.Database.SetDbConnection(connection);
                            await _dbContext.Database.UseTransactionAsync(transaction, cancellationToken);
                            await _dbContext.Records
                                .Where(x => handler.UpdateRecords.Select(x => x.Id).Contains(x.Id.ToString()))
                                .ExecuteUpdateAsync(te => te.SetProperty(e => e.UpdatedAt, e => e.UpdatedAt));
                            await _dbContext.FlexibleRecords
                                .Where(x => handler.RemoveFlexibleRecords.Contains(x.Id))
                                .ExecuteDeleteAsync(cancellationToken);
                            await _dbContext.Edges
                                .Where(x => handler.RemoveEdgeRecords.Contains(x.Id))
                                .ExecuteDeleteAsync(cancellationToken);
                            await _dbContext.Records
                                .Where(x => handler.RemoveRecords.Contains(x.Id))
                                .ExecuteDeleteAsync(cancellationToken);
                            await BulkInsertTermEntriesAsync(connection, transaction, handler.NewTermEntries, cancellationToken);
                            await BulkInsertEdgesAsync(connection, transaction, handler.NewEdgeRecords, cancellationToken);
                            await BulkUpdateEdgesAsync(connection, transaction, handler.UpdateEdgeRecords, cancellationToken);
                            await BulkInsertFlexibleRecordsAsync(connection, transaction, handler.NewFlexibleRecords, cancellationToken);
                            await BulkUpdateFlexibleRecordsAsync(connection, transaction, handler.UpdateFlexibleRecords, cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            // Rollback transaction in case of an error
                            await transaction.RollbackAsync(cancellationToken);
                            return ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO logging
                return ex.Message;
            }

            return new Result<IReference>(handler.MainReference);
        }

        protected async Task BulkUpdateFlexibleRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            List<Postgres.Models.FlexibleRecord> flexibleRecordsToUpdate,
            CancellationToken cancellationToken)
        {
            if (flexibleRecordsToUpdate == null || flexibleRecordsToUpdate.Count == 0)
                return;

            // Step 1: Create a temporary table
            var createTempTableCommand = @"
    CREATE TEMP TABLE TempFlexibleRecords (
        ""Id"" UUID NOT NULL,
        ""DataType"" TEXT NOT NULL,
        ""Data"" JSON NOT NULL
    ) ON COMMIT DROP;";

            using (var command = new NpgsqlCommand(createTempTableCommand, connection, transaction))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            // Step 2: Insert data into the temporary table
            using (var writer = connection.BeginBinaryImport(@"
        COPY TempFlexibleRecords (""Id"", ""DataType"", ""Data"")
        FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Postgres.Models.FlexibleRecord record in flexibleRecordsToUpdate)
                {
                    writer.StartRow();
                    writer.Write(record.Id, NpgsqlTypes.NpgsqlDbType.Uuid);
                    writer.Write(record.DataType, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(record.Data, NpgsqlTypes.NpgsqlDbType.Json);
                }
                writer.Complete();
            }

            // Step 3: Perform the bulk update using a join
            var updateCommand = @"
    UPDATE public.""FlexibleRecords"" AS fr
    SET
        ""DataType"" = t.""DataType"",
        ""Data"" = t.""Data""
    FROM TempFlexibleRecords AS t
    WHERE fr.""Id"" = t.""Id"";";

            using (var command = new NpgsqlCommand(updateCommand, connection, transaction))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }


        protected async Task BulkUpdateEdgesAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            List<Postgres.Models.Edge> edgesToUpdate,
            CancellationToken cancellationToken)
        {
            if (edgesToUpdate == null || edgesToUpdate.Count == 0)
                return;

            // Step 1: Create a temporary table
            var createTempTableCommand = @"
    CREATE TEMP TABLE TempEdges (
        ""Id"" UUID NOT NULL,
        ""SourceNodeId"" UUID NOT NULL,
        ""TargetNodeId"" UUID NOT NULL,
        ""EdgeType"" INTEGER NOT NULL,
        ""Active"" BOOLEAN NOT NULL,
        ""ParentNodeId"" UUID NULL,
        ""RootNodeId"" UUID NOT NULL,
        ""Order"" INTEGER NULL
    ) ON COMMIT DROP;";

            using (var command = new NpgsqlCommand(createTempTableCommand, connection, transaction))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            // Step 2: Insert data into the temporary table
            using (var writer = connection.BeginBinaryImport(@"
        COPY TempEdges (""Id"", ""SourceNodeId"", ""TargetNodeId"", ""EdgeType"", ""Active"", ""ParentNodeId"", ""RootNodeId"", ""Order"")
        FROM STDIN (FORMAT BINARY)"))
            {
                foreach (Postgres.Models.Edge edge in edgesToUpdate)
                {
                    writer.StartRow();
                    writer.Write(edge.Id, NpgsqlTypes.NpgsqlDbType.Uuid);
                    writer.Write(edge.SourceNodeId, NpgsqlTypes.NpgsqlDbType.Uuid);
                    writer.Write(edge.TargetNodeId, NpgsqlTypes.NpgsqlDbType.Uuid);
                    writer.Write(edge.EdgeType, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(edge.Active, NpgsqlTypes.NpgsqlDbType.Boolean);
                    writer.Write(edge.ParentNodeId.HasValue ? (object)edge.ParentNodeId.Value : DBNull.Value, NpgsqlTypes.NpgsqlDbType.Uuid);
                    writer.Write(edge.RootNodeId, NpgsqlTypes.NpgsqlDbType.Uuid);
                    writer.Write(edge.Order.HasValue ? (object)edge.Order.Value : DBNull.Value, NpgsqlTypes.NpgsqlDbType.Integer);
                }
                writer.Complete();
            }

            // Step 3: Perform the bulk update using a join
            var updateCommand = @"
    UPDATE public.""Edges"" AS e
    SET
        ""SourceNodeId"" = t.""SourceNodeId"",
        ""TargetNodeId"" = t.""TargetNodeId"",
        ""EdgeType"" = t.""EdgeType"",
        ""Active"" = t.""Active"",
        ""ParentNodeId"" = t.""ParentNodeId"",
        ""RootNodeId"" = t.""RootNodeId"",
        ""Order"" = t.""Order""
    FROM TempEdges AS t
    WHERE e.""Id"" = t.""Id"";";

            using (var command = new NpgsqlCommand(updateCommand, connection, transaction))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }


        public async Task BulkInsertFlexibleRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            List<Postgres.Models.FlexibleRecord> flexibleRecords,
            CancellationToken cancellationToken)
        {
            var batchCommand = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();

            int counter = 0;
            foreach (Postgres.Models.FlexibleRecord record in flexibleRecords)
            {
                // Add the SQL statement to the batch
                batchCommand.Append($@"
        INSERT INTO public.""FlexibleRecords"" 
        (""Id"", ""DataType"", ""Data"") 
        VALUES 
        (@Id{counter}, @DataType{counter}, @Data{counter});");

                // Add parameters directly
                parameters.Add(new NpgsqlParameter($"@Id{counter}", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = record.Id });
                parameters.Add(new NpgsqlParameter($"@DataType{counter}", NpgsqlTypes.NpgsqlDbType.Text) { Value = record.DataType });
                parameters.Add(new NpgsqlParameter($"@Data{counter}", NpgsqlTypes.NpgsqlDbType.Json) { Value = record.Data });

                counter++;

                // Execute batch when the batch size is reached
                if (counter % BATCH_SIZE == 0)
                {
                    await ExecuteBatchAsync(connection, transaction, batchCommand, parameters, cancellationToken);
                    batchCommand.Clear();
                    parameters.Clear();
                }
            }

            // Execute remaining records in the last batch
            if (batchCommand.Length > 0)
            {
                await ExecuteBatchAsync(connection, transaction, batchCommand, parameters, cancellationToken);
            }
        }

        protected async Task BulkInsertEdgesAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            List<Postgres.Models.Edge> edges,
            CancellationToken cancellationToken)
        {
            var batchCommand = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();

            int counter = 0;
            foreach (Postgres.Models.Edge edge in edges)
            {
                // Add the SQL statement to the batch
                batchCommand.Append($@"
        INSERT INTO public.""Edges"" 
        (""Id"", ""SourceNodeId"", ""TargetNodeId"", ""EdgeType"", ""Active"", ""ParentNodeId"", ""RootNodeId"", ""Order"") 
        VALUES 
        (@Id{counter}, @SourceNodeId{counter}, @TargetNodeId{counter}, @EdgeType{counter}, 
        @Active{counter}, @ParentNodeId{counter}, @RootNodeId{counter}, @Order{counter});");

                // Add parameters directly
                parameters.Add(new NpgsqlParameter($"@Id{counter}", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = edge.Id });
                parameters.Add(new NpgsqlParameter($"@SourceNodeId{counter}", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = edge.SourceNodeId });
                parameters.Add(new NpgsqlParameter($"@TargetNodeId{counter}", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = edge.TargetNodeId });
                parameters.Add(new NpgsqlParameter($"@EdgeType{counter}", NpgsqlTypes.NpgsqlDbType.Integer) { Value = edge.EdgeType });
                parameters.Add(new NpgsqlParameter($"@Active{counter}", NpgsqlTypes.NpgsqlDbType.Boolean) { Value = edge.Active });
                parameters.Add(new NpgsqlParameter($"@ParentNodeId{counter}", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = (object)edge.ParentNodeId ?? DBNull.Value });
                parameters.Add(new NpgsqlParameter($"@RootNodeId{counter}", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = edge.RootNodeId });
                parameters.Add(new NpgsqlParameter($"@Order{counter}", NpgsqlTypes.NpgsqlDbType.Integer) { Value = (object)edge.Order ?? DBNull.Value });

                counter++;

                // Execute batch when the batch size is reached
                if (counter % BATCH_SIZE == 0)
                {
                    await ExecuteBatchAsync(connection, transaction, batchCommand, parameters, cancellationToken);
                    batchCommand.Clear();
                    parameters.Clear();
                }
            }

            // Execute remaining records in the last batch
            if (batchCommand.Length > 0)
            {
                await ExecuteBatchAsync(connection, transaction, batchCommand, parameters, cancellationToken);
            }
        }

        protected async Task ExecuteBatchAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            StringBuilder batchCommand,
            List<NpgsqlParameter> parameters,
            CancellationToken cancellationToken)
        {
            using (var command = new NpgsqlCommand(batchCommand.ToString(), connection, transaction))
            {
                command.Parameters.AddRange(parameters.ToArray()); // Add all parameters in one go
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        protected async Task BulkInsertTermEntriesAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            List<Postgres.Models.TermEntry> termEntries,
            CancellationToken cancellationToken)
        {
            var batchCommand = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();

            int counter = 0;
            foreach (Postgres.Models.TermEntry entry in termEntries)
            {
                // Add the SQL statement to the batch
                batchCommand.Append($@"
        INSERT INTO public.""TermEntries"" 
        (""Id"", ""Title"", ""Label"") 
        VALUES 
        (@Id{counter}, @Title{counter}, @Label{counter});");

                // Add parameters directly
                parameters.Add(new NpgsqlParameter($"@Id{counter}", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = entry.Id });
                parameters.Add(new NpgsqlParameter($"@Title{counter}", NpgsqlTypes.NpgsqlDbType.Text) { Value = entry.Title });
                parameters.Add(new NpgsqlParameter($"@Label{counter}", NpgsqlTypes.NpgsqlDbType.Text) { Value = entry.Label });

                counter++;

                // Execute batch when the batch size is reached
                if (counter % BATCH_SIZE == 0)
                {
                    await ExecuteBatchAsync(connection, transaction, batchCommand, parameters, cancellationToken);
                    batchCommand.Clear();
                    parameters.Clear();
                }
            }

            // Execute remaining records in the last batch
            if (batchCommand.Length > 0)
            {
                await ExecuteBatchAsync(connection, transaction, batchCommand, parameters, cancellationToken);
            }
        }

        protected async Task BulkInsertRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            List<Record> records,
            CancellationToken cancellationToken)
        {
            var batchCommand = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();

            int counter = 0;
            foreach (Record record in records)
            {
                // Add the SQL statement to the batch
                batchCommand.Append($@"
        INSERT INTO public.""Records"" 
        (""Id"", ""Status"", ""CreatedAt"", ""CreatedBy"", ""UpdatedAt"", ""UpdatedBy"", ""ProviderName"", ""ExternalId"") 
        VALUES 
        (@Id{counter}, @Status{counter}, @CreatedAt{counter}, @CreatedBy{counter}, 
        @UpdatedAt{counter}, @UpdatedBy{counter}, @ProviderName{counter}, @ExternalId{counter});");

                // Add parameters directly
                parameters.Add(new NpgsqlParameter($"@Id{counter}", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = record.Id });
                parameters.Add(new NpgsqlParameter($"@Status{counter}", NpgsqlTypes.NpgsqlDbType.Integer) { Value = record.Status });
                parameters.Add(new NpgsqlParameter($"@CreatedAt{counter}", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = record.CreatedAt });
                parameters.Add(new NpgsqlParameter($"@CreatedBy{counter}", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = record.CreatedBy });
                parameters.Add(new NpgsqlParameter($"@UpdatedAt{counter}", NpgsqlTypes.NpgsqlDbType.TimestampTz) { Value = record.UpdatedAt });
                parameters.Add(new NpgsqlParameter($"@UpdatedBy{counter}", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = record.UpdatedBy });
                parameters.Add(new NpgsqlParameter($"@ProviderName{counter}", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = record.ProviderName });
                parameters.Add(new NpgsqlParameter($"@ExternalId{counter}", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = (object)record.ExternalId ?? DBNull.Value });

                counter++;

                // Execute batch when the batch size is reached
                if (counter % BATCH_SIZE == 0)
                {
                    await ExecuteBatchAsync(connection, transaction, batchCommand, parameters, cancellationToken);
                    batchCommand.Clear();
                    parameters.Clear();
                }
            }

            // Execute remaining records in the last batch
            if (batchCommand.Length > 0)
            {
                await ExecuteBatchAsync(connection, transaction, batchCommand, parameters, cancellationToken);
            }
        }

        private async Task CopyTextIntoRecordsAsync(string connectionString, HashSet<Postgres.Models.Record> records)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Create a memory stream to write data
                using (TextWriter writer = await connection.BeginTextImportAsync("COPY public.\"Records\" (\"Id\", \"Status\", \"CreatedAt\", \"CreatedBy\", \"UpdatedAt\", \"UpdatedBy\", \"ProviderName\", \"ExternalId\") FROM STDIN (FORMAT CSV)"))
                {
                    var sb = new StringBuilder();

                    foreach (var record in records)
                    {
                        sb.Clear();
                        sb.Append($"{record.Id},");
                        sb.Append($"{record.Status},");
                        sb.Append($"{record.CreatedAt:yyyy-MM-dd HH:mm:ssZ},");
                        sb.Append($"{record.CreatedBy},");
                        sb.Append($"{record.UpdatedAt:yyyy-MM-dd HH:mm:ssZ},");
                        sb.Append($"{record.UpdatedBy},");
                        sb.Append($"{record.ProviderName},");
                        sb.Append($"{(string.IsNullOrEmpty(record.ExternalId) ? "" : record.ExternalId)}");
                        sb.Append("\n");

                        writer.Write(sb.ToString());
                    }
                }
            }
        }
        private async Task CopyBinaryIntoRecordsAsync(string connectionString, HashSet<Postgres.Models.Record> records)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var writer = connection.BeginBinaryImport(@"
                COPY public.""Records"" 
                (""Id"", ""Status"", ""CreatedAt"", ""CreatedBy"", ""UpdatedAt"", ""UpdatedBy"", ""ProviderName"", ""ExternalId"")
                FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var record in records)
                    {
                        writer.StartRow();
                        writer.Write(record.Id, NpgsqlTypes.NpgsqlDbType.Uuid);
                        writer.Write(record.Status, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(record.CreatedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz);
                        writer.Write(record.CreatedBy, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(record.UpdatedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz);
                        writer.Write(record.UpdatedBy, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(record.ProviderName, NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(record.ExternalId, NpgsqlTypes.NpgsqlDbType.Varchar); // Handles null automatically
                    }

                    writer.Complete();
                }
            }
        }

    }
}
