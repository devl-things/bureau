using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Data.Postgres.Configurations;
using Bureau.Data.Postgres.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Bureau.Data.Postgres.Repositories
{
    internal class EdgeTypeSearchQueryRepository : BaseRecordQueryRepository, IRecordQueryRepository<EdgeTypeSearchRequest, QueryAggregateModel>
    {
        private readonly ILogger<EdgeTypeSearchQueryRepository> _logger;
        private readonly BureauDataOptions _options;

        private EdgeTypeSearchRequest _edgeTypeSearchRequest;

        public EdgeTypeSearchQueryRepository(ILogger<EdgeTypeSearchQueryRepository> logger, IOptions<BureauDataOptions> options) : base()
        {
            _logger = logger;
            _options = options.Value;
            ConnectionString = _options.ConnectionString;
        }

        public async Task<Result<QueryAggregateModel>> FetchRecordsAsync(EdgeTypeSearchRequest edgeTypeSearchRequest, CancellationToken cancellationToken)
        {
            _edgeTypeSearchRequest = edgeTypeSearchRequest;
            SelectReferences = _edgeTypeSearchRequest.SelectReferences;
            SelectRecordTypes = _edgeTypeSearchRequest.SelectRecordTypes;
            Pagination = _edgeTypeSearchRequest.Pagination;

            Result<QueryAggregateModel> aggregateModelResult = await FetchRecordsAsync(cancellationToken);
            if (aggregateModelResult.IsError)
            {
                return aggregateModelResult.Error;
            }
            return aggregateModelResult.Value;
        }

        protected override async Task<List<Edge>> FilterEdgesAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
        {
            const string edgesCondition = $"in (SELECT \"Id\" FROM FilteredEdges)";
            string edgesWhereClause = PrepareEdgeFilterType(_edgeTypeSearchRequest.FilterRequestType, edgesCondition);

            string basicWhereClause = "WHERE \"EdgeType\" = @EdgeType";
            if (_edgeTypeSearchRequest.Active.HasValue)
            {
                basicWhereClause = $"{basicWhereClause} AND \"Active\" = @EdgeActive";
            }

            string countQuery = $@"SELECT count(""Id"") FROM public.""Edges"" {basicWhereClause}";

            string query = $@"
    WITH FilteredEdges AS (
        SELECT e.""Id"" 
        FROM public.""Edges"" e
	        INNER JOIN public.""Records"" r ON e.""Id"" = r.""Id""
        {basicWhereClause}
        ORDER BY ""r"".""CreatedAt"" desc
        OFFSET {_edgeTypeSearchRequest.Pagination.Offset} LIMIT {_edgeTypeSearchRequest.Pagination.Limit}  )
    {SELECT_EDGES}
    WHERE {edgesWhereClause}";

            _logger.Debug(query);
            List<Postgres.Models.Edge> edges = new List<Postgres.Models.Edge>();
            using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@EdgeType", NpgsqlTypes.NpgsqlDbType.Integer, _edgeTypeSearchRequest.EdgeType);
                if (_edgeTypeSearchRequest.Active.HasValue)
                {
                    command.Parameters.AddWithValue("@EdgeActive", NpgsqlTypes.NpgsqlDbType.Boolean, _edgeTypeSearchRequest.Active.Value);
                }

                using (NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        edges.Add(ReadEdge(reader));
                    }
                }

                command.CommandText = countQuery;
                object? scalarResult = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (scalarResult != null && scalarResult != DBNull.Value) 
                {
                    Pagination.TotalItems = Convert.ToInt32(scalarResult);
                }
            }

            return edges;
        }
    }
}
