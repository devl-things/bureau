using Bureau.Core;
using Bureau.Core.Comparers;
using Bureau.Core.Configuration;
using Bureau.Core.Extensions;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Data.Postgres.Configurations;
using Bureau.Data.Postgres.Contexts;
using Bureau.Data.Postgres.Handlers;
using Bureau.Data.Postgres.Mappers;
using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using dbModels = Bureau.Data.Postgres.Models;

namespace Bureau.Data.Postgres.Repositories
{
    internal class RecordCommandRepository : IRecordCommandRepository, ITermRepository
    {
        private BureauOptions _options;

        private readonly ILogger<RecordCommandRepository> _logger;

        private readonly BureauContext _dbContext;
        private readonly string? _connectionString;
        public RecordCommandRepository(ILogger<RecordCommandRepository> logger, IOptions<BureauOptions> options, BureauContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _options = options.Value;
            _connectionString = _dbContext.Database.GetConnectionString();
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

        public async Task<Result> DeleteAggregateAsync(IRemoveAggregateModel request, CancellationToken cancellationToken) 
        {
            RemoveModelHandler handler = new RemoveModelHandler(request);
            Result result = handler.HandleAggregate();
            if (result.IsError)
            {
                return result.Error;
            }
            try
            {
                using (IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        await _dbContext.FlexibleRecords
                            .Where(x => handler.RemoveFlexibleRecords.Contains(x.Id))
                            .ExecuteDeleteAsync(cancellationToken);
                        await _dbContext.Edges
                            .Where(x => handler.RemoveEdgeRecords.Contains(x.Id))
                            .ExecuteDeleteAsync(cancellationToken);
                        await _dbContext.Records
                            .Where(x => handler.RemoveRecords.Contains(x.Id))
                            .ExecuteDeleteAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogError(exception: ex, nameof(DeleteAggregateAsync));
                        return ResultErrorFactory.UnexpectedError();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, nameof(DeleteAggregateAsync));
                return ResultErrorFactory.UnexpectedError();
            }
            return true;
        }

        public async Task<Result<IReference>> InsertAggregateAsync(InsertAggregateModel insertRequest, CancellationToken cancellationToken)
        {
            InsertModelHandler handler = new InsertModelHandler(insertRequest);
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
                            await BulkMergeRecordsAsync(connection, transaction, handler.Records, cancellationToken);
                            await BulkInsertTermEntriesAsync(connection, transaction, handler.NewTermEntries, cancellationToken);
                            await BulkInsertEdgesAsync(connection, transaction, handler.NewEdgeRecords, cancellationToken);
                            await BulkUpdateEdgesAsync(connection, transaction, handler.UpdateEdgeRecords, cancellationToken);
                            await BulkInsertFlexibleRecordsAsync(connection, transaction, handler.NewFlexibleRecords, cancellationToken);
                            await BulkMergeFlexibleRecordsAsync(connection, transaction, handler.UpdateFlexibleRecords, cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            _logger.LogError(exception: ex, nameof(InsertAggregateAsync));
                            return ResultErrorFactory.UnexpectedError();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, nameof(InsertAggregateAsync));
                return ResultErrorFactory.UnexpectedError();
            }

            return new Result<IReference>(handler.MainReference);
        }

        public async Task<Result<IReference>> UpdateAggregateAsync(UpdateAggregateModel updateRequest, CancellationToken cancellationToken)
        {
            UpdateModelHandler handler = new UpdateModelHandler(updateRequest);
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
                            await BulkMergeRecordsAsync(connection, transaction, handler.Records, cancellationToken);
                            _dbContext.Database.SetDbConnection(connection);
                            await _dbContext.Database.UseTransactionAsync(transaction, cancellationToken);
                            await _dbContext.FlexibleRecords
                                .Where(x => handler.RemoveFlexibleRecords.Contains(x.Id))
                                .ExecuteDeleteAsync(cancellationToken);
                            await _dbContext.Edges
                                .Where(x => handler.RemoveEdgeRecords.Contains(x.Id))
                                .ExecuteDeleteAsync(cancellationToken);
                            //TODO [IMPROVE] [first] no removing of the records because we cannot know that record is not used someplace else,
                            //there should be a separate service to handle that
                            await _dbContext.Records
                                .Where(x => handler.RemoveRecords.Contains(x.Id))
                                .ExecuteDeleteAsync(cancellationToken);
                            await BulkInsertTermEntriesAsync(connection, transaction, handler.NewTermEntries, cancellationToken);
                            await BulkInsertEdgesAsync(connection, transaction, handler.NewEdgeRecords, cancellationToken);
                            await BulkUpdateEdgesAsync(connection, transaction, handler.UpdateEdgeRecords, cancellationToken);
                            await BulkInsertFlexibleRecordsAsync(connection, transaction, handler.NewFlexibleRecords, cancellationToken);
                            await BulkMergeFlexibleRecordsAsync(connection, transaction, handler.UpdateFlexibleRecords, cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            _logger.LogError(exception: ex, nameof(UpdateAggregateAsync));
                            return ResultErrorFactory.UnexpectedError();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, nameof(UpdateAggregateAsync));
                return ResultErrorFactory.UnexpectedError();
            }

            return new Result<IReference>(handler.MainReference);
        }
        //TODO [IMPROVE] [first] use copy command not multiple inserts
        //TODO [IMPROVE] [first] with merge there is no need to have insert and update separated
        protected async Task BulkMergeFlexibleRecordsAsync(
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
    MERGE INTO public.""FlexibleRecords"" AS target
    USING TempFlexibleRecords AS source
    ON target.""Id"" = source.""Id""
    WHEN MATCHED THEN
        UPDATE SET ""DataType"" = source.""DataType"", ""Data"" = source.""Data""
    WHEN NOT MATCHED THEN
        INSERT (""Id"", ""DataType"", ""Data"")
        VALUES (source.""Id"", source.""DataType"", source.""Data"");";
            //TIPS this is for older versions of postgres < v15
            //INSERT INTO ""FlexibleRecords"" ("Id", "DataType", "Data")
            //SELECT "Id", "DataType", "Data"
            //FROM ""TempFlexibleRecords""
            //ON CONFLICT("Id") 
            //DO UPDATE
            //SET "DataType" = EXCLUDED."DataType",
            //    "Data" = EXCLUDED."Data";
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
                if (counter % _options.BatchProcessingSize == 0)
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
                if (counter % _options.BatchProcessingSize == 0)
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
                if (counter % _options.BatchProcessingSize == 0)
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
                if (counter % _options.BatchProcessingSize == 0)
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

        protected async Task BulkMergeRecordsAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            List<Record> recordsToUpdate,
            CancellationToken cancellationToken)
        {
            if (recordsToUpdate == null || recordsToUpdate.Count == 0)
                return;

            // Step 1: Create a temporary table
            var createTempTableCommand = @"
            CREATE TEMP TABLE TempRecords (
                ""Id"" UUID NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""CreatedAt"" TIMESTAMPTZ NOT NULL,
                ""CreatedBy"" VARCHAR(40) NOT NULL,
                ""UpdatedAt"" TIMESTAMPTZ NOT NULL,
                ""UpdatedBy"" VARCHAR(40) NOT NULL,
                ""ProviderName"" VARCHAR(20) NOT NULL,
                ""ExternalId"" VARCHAR(40) NULL
            ) ON COMMIT DROP; ";

            using (var command = new NpgsqlCommand(createTempTableCommand, connection, transaction))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            // Step 2: Insert data into the temporary table
            using (var writer = connection.BeginBinaryImport(@"
            COPY TempRecords (""Id"", ""Status"", ""CreatedAt"", ""CreatedBy"", ""UpdatedAt"", ""UpdatedBy"", ""ProviderName"", ""ExternalId"")
            FROM STDIN(FORMAT BINARY)"))
            {
                foreach (var record in recordsToUpdate)
                {
                    writer.StartRow();
                    writer.Write(record.Id, NpgsqlTypes.NpgsqlDbType.Uuid);
                    writer.Write(record.Status, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(record.CreatedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz);
                    writer.Write(record.CreatedBy, NpgsqlTypes.NpgsqlDbType.Varchar);
                    writer.Write(record.UpdatedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz);
                    writer.Write(record.UpdatedBy, NpgsqlTypes.NpgsqlDbType.Varchar);
                    writer.Write(record.ProviderName, NpgsqlTypes.NpgsqlDbType.Varchar);
                    writer.Write(record.ExternalId, NpgsqlTypes.NpgsqlDbType.Varchar);

                }
                writer.Complete();
            }

            // Step 3: Perform the bulk update using a join
            var updateCommand = @"
            MERGE INTO public.""Records"" AS target
            USING TempRecords AS source
            ON target.""Id"" = source.""Id""
            WHEN MATCHED THEN
                UPDATE SET
                    ""Status"" = source.""Status"", 
                    ""UpdatedAt"" = source.""UpdatedAt"", 
                    ""UpdatedBy"" = source.""UpdatedBy""
            WHEN NOT MATCHED THEN
                INSERT(""Id"", ""Status"", ""CreatedAt"", ""CreatedBy"", ""UpdatedAt"", ""UpdatedBy"", ""ProviderName"", ""ExternalId"")
                VALUES(source.""Id"", source.""Status"", source.""CreatedAt"", source.""CreatedBy"", source.""UpdatedAt"", source.""UpdatedBy"", 
                    source.""ProviderName"", source.""ExternalId"");";

            using (var command = new NpgsqlCommand(updateCommand, connection, transaction))
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
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
