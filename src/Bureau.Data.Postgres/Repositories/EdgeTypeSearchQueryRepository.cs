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
    internal class EdgeTypeSearchQueryRepository : BaseRecordQueryRepository, IRecordQueryRepository<EdgeTypeSearchRequest, BaseAggregateModel>
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

        public async Task<Result<BaseAggregateModel>> FetchRecordsAsync(EdgeTypeSearchRequest edgeTypeSearchRequest, CancellationToken cancellationToken)
        {
            _edgeTypeSearchRequest = edgeTypeSearchRequest;
            SelectReferences = _edgeTypeSearchRequest.SelectReferences;
            SelectRecordTypes = _edgeTypeSearchRequest.SelectRecordTypes;

            Result<BaseAggregateModel> aggregateModelResult = await FetchRecordsAsync(cancellationToken);
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

            string query = $@"
    WITH FilteredEdges AS (SELECT ""Id"" FROM public.""Edges"" {basicWhereClause})
    {SELECT_EDGES}
    WHERE {edgesWhereClause}";

            _logger.Debug(query);
            List<Postgres.Models.Edge> edges = new List<Postgres.Models.Edge>();
            using (var command = new NpgsqlCommand(query, connection))
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
            }

            return edges;
        }
    }
}
