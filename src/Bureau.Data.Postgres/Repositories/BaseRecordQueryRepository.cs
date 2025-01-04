using Bureau.Core.Models.Data;
using Bureau.Core;
using Bureau.Data.Postgres.Models;
using Npgsql;
using Bureau.Core.Comparers;
using Bureau.Core.Models;
using Bureau.Data.Postgres.Mappers;

namespace Bureau.Data.Postgres.Repositories
{
    internal abstract class BaseRecordQueryRepository
    {
        protected const string SELECT_EDGES = @"
        SELECT 
            ""Id"", 
            ""SourceNodeId"", 
            ""TargetNodeId"", 
            ""EdgeType"", 
            ""Active"", 
            ""ParentNodeId"", 
            ""RootNodeId"", 
            ""Order""
        FROM public.""Edges""";

        private List<Postgres.Models.Edge> _filteredEdges;
        private HashSet<Guid> _relatedRecordIds;

        private string _connectionString = default!;

        private EdgeRequestType _selectReferences = default;
        private RecordRequestType _selectRecordTypes = default;

        protected BaseRecordQueryRepository()
        {
            _filteredEdges = new List<Postgres.Models.Edge>();
            _relatedRecordIds = new HashSet<Guid>();
        }

        public string ConnectionString { get { return _connectionString; }  set { _connectionString = value; } }

        protected PaginationMetadata Pagination { get; set; } = new PaginationMetadata();

        protected EdgeRequestType SelectReferences { get { return _selectReferences; } set { _selectReferences = value; } }
        protected RecordRequestType SelectRecordTypes { get { return _selectRecordTypes; } set { _selectRecordTypes = value; } }
        public async Task<Result<QueryAggregateModel>> FetchRecordsAsync(CancellationToken cancellationToken) 
        {
            if (string.IsNullOrWhiteSpace(_connectionString)) 
            {
                return "Connection string is required";
            }
            QueryAggregateModel aggregateModel = new QueryAggregateModel(Pagination);
            Dictionary<Guid, Record> records;
            // Step 1: Create the main connection for filtering edges and fetching records
            await using (NpgsqlConnection mainConnection = new NpgsqlConnection(_connectionString))
            {
                await mainConnection.OpenAsync(cancellationToken);

                // Step 2: Filter Edges based on FilterReferenceId and FilterRequestType
                _filteredEdges = await FilterEdgesAsync(mainConnection, cancellationToken);

                // Step 3: Gather all related Record IDs based on SelectReferences
                foreach (Postgres.Models.Edge edge in _filteredEdges)
                {
                    if (_selectReferences.HasFlag(EdgeRequestType.Edge))
                        _relatedRecordIds.Add(edge.Id);

                    if (_selectReferences.HasFlag(EdgeRequestType.SourceNode))
                        _relatedRecordIds.Add(edge.SourceNodeId);

                    if (_selectReferences.HasFlag(EdgeRequestType.TargetNode))
                        _relatedRecordIds.Add(edge.TargetNodeId);

                    if (_selectReferences.HasFlag(EdgeRequestType.RootNode))
                        _relatedRecordIds.Add(edge.RootNodeId);
                }

                // Step 4: Fetch related Records
                records = await FetchRecordsByIdsAsync(mainConnection, _relatedRecordIds, cancellationToken);
            }
            // Step 5: Populate additional entities based on SelectRecordTypes using separate connections
            var tasks = new List<Task>();

            if (_selectRecordTypes.HasFlag(RecordRequestType.Edges))
            {
                tasks.Add(Task.Run(() =>
                {
                    _filteredEdges.ForEach(x => x.Record = records[x.Id]);
                    IEnumerable<Core.Models.Edge> r = _filteredEdges.Select(x => x.ToModel());
                    aggregateModel.Edges = new HashSet<Core.Models.Edge>(r, new ReferenceComparer());
                }, cancellationToken));
            }

            if (_selectRecordTypes.HasFlag(RecordRequestType.TermEntries))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await using (var termConnection = new NpgsqlConnection(_connectionString))
                    {
                        await termConnection.OpenAsync(cancellationToken);

                        var termEntries = await FetchTermEntriesByIdsAsync(termConnection, _relatedRecordIds, records, cancellationToken);
                        IEnumerable<Core.Models.TermEntry> r = termEntries.Select(x => x.ToModel());
                        aggregateModel.TermEntries = new HashSet<Core.Models.TermEntry>(r, new ReferenceComparer());
                    }
                }, cancellationToken));
            }

            if (_selectRecordTypes.HasFlag(RecordRequestType.FlexRecords))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await using (var flexConnection = new NpgsqlConnection(_connectionString))
                    {
                        await flexConnection.OpenAsync(cancellationToken);

                        HashSet<FlexibleRecord> flexRecords = await FetchFlexibleRecordsByIdsAsync(flexConnection, _relatedRecordIds, records, cancellationToken);
                        IEnumerable<FlexRecord> r = flexRecords.Select(flexRecord => flexRecord.ToModel());
                        aggregateModel.FlexRecords = new HashSet<FlexRecord>(r, new ReferenceComparer());
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            return aggregateModel;
        }

        public bool RecordExists(string id) 
        {
            if (Guid.TryParse(id, out Guid recordId))
            {
                return _relatedRecordIds.Contains(recordId);
            }
            return false;
        }
        protected abstract Task<List<Postgres.Models.Edge>> FilterEdgesAsync(NpgsqlConnection mainConnection, CancellationToken cancellationToken);

        protected static string PrepareEdgeFilterType(EdgeRequestType edgeRequestType, string edgeCondition)
        {
            List<string> filterConditions = new List<string>();

            if (edgeRequestType.HasFlag(EdgeRequestType.Edge))
                filterConditions.Add($"\"Id\" {edgeCondition}");

            if (edgeRequestType.HasFlag(EdgeRequestType.SourceNode))
                filterConditions.Add($"\"SourceNodeId\" {edgeCondition}");

            if (edgeRequestType.HasFlag(EdgeRequestType.TargetNode))
                filterConditions.Add($"\"TargetNodeId\" {edgeCondition}");

            if (edgeRequestType.HasFlag(EdgeRequestType.RootNode))
                filterConditions.Add($"\"RootNodeId\" {edgeCondition}");

            return string.Join(" OR ", filterConditions);
        }

        protected static Postgres.Models.Edge ReadEdge(NpgsqlDataReader reader)
        {
            return new Postgres.Models.Edge
            {
                Id = reader.GetGuid(0),
                SourceNodeId = reader.GetGuid(1),
                TargetNodeId = reader.GetGuid(2),
                EdgeType = reader.GetInt32(3),
                Active = reader.GetBoolean(4),
                ParentNodeId = reader.IsDBNull(5) ? (Guid?)null : reader.GetGuid(5),
                RootNodeId = reader.GetGuid(6),
                Order = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7)
            };
        }

        private static async Task<Dictionary<Guid, Record>> FetchRecordsByIdsAsync(
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

        private static async Task<HashSet<Postgres.Models.TermEntry>> FetchTermEntriesByIdsAsync(
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

        private static async Task<HashSet<FlexibleRecord>> FetchFlexibleRecordsByIdsAsync(
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
    }
}
