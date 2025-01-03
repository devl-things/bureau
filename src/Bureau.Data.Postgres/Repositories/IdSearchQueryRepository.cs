using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Core.Factories;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Data.Postgres.Configurations;
using Bureau.Data.Postgres.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Bureau.Data.Postgres.Repositories
{
    internal class IdSearchQueryRepository : BaseRecordQueryRepository, IRecordQueryRepository<IdSearchRequest, AggregateModel>
    {
        private readonly ILogger<IdSearchQueryRepository> _logger;
        private readonly BureauDataOptions _options;

        private IdSearchRequest _idSearchRequest;
        public IdSearchQueryRepository(ILogger<IdSearchQueryRepository> logger, IOptions<BureauDataOptions> options) : base()
        {
            _logger = logger;
            _options = options.Value;
            ConnectionString = _options.ConnectionString;
        }

        public async Task<Result<AggregateModel>> FetchRecordsAsync(IdSearchRequest idSearchRequest, CancellationToken cancellationToken)
        {
            if (idSearchRequest.FilterReferenceId is null)
            {
                return "Reference Id is required";
            }
            _idSearchRequest = idSearchRequest;
            SelectReferences = _idSearchRequest.SelectReferences;
            SelectRecordTypes = _idSearchRequest.SelectRecordTypes;

            Result<BaseAggregateModel> aggregateModelResult = await FetchRecordsAsync(cancellationToken);
            if (aggregateModelResult.IsError) 
            {
                return aggregateModelResult.Error;
            }
            if (!RecordExists(_idSearchRequest.FilterReferenceId.Id)) 
            {
                return "Reference Id not found in the database";
            }

            AggregateModel result = new AggregateModel(aggregateModelResult.Value)
            {
                MainReference = BureauReferenceFactory.CreateReference(_idSearchRequest.FilterReferenceId.Id)
            };
            return result;
        }

        protected override async Task<List<Edge>> FilterEdgesAsync(NpgsqlConnection mainConnection, CancellationToken cancellationToken)
        {
            // Build dynamic WHERE clause based on FilterRequestType flags
            // Combine conditions into a single WHERE clause
            string whereClause = PrepareEdgeFilterType(_idSearchRequest.FilterRequestType, "= @ReferenceId");

            string query = $@"
        {SELECT_EDGES}
        WHERE {whereClause};";

            _logger.Debug(query);

            List<Postgres.Models.Edge> edges = new List<Postgres.Models.Edge>();
            using (var command = new NpgsqlCommand(query, mainConnection))
            {
                command.Parameters.AddWithValue("@ReferenceId", NpgsqlTypes.NpgsqlDbType.Uuid, Guid.Parse(_idSearchRequest.FilterReferenceId.Id));

                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
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
