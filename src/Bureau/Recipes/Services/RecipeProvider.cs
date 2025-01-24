using Bureau.Core;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Core.Services;
using Bureau.Factories;
using Bureau.Models;
using Bureau.Providers;
using Bureau.Recipes.Factories;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Services
{
    internal class RecipeProvider : IDtoProvider<RecipeDto>, IInternalRecipeQueryHandler
    {
        private readonly IPaginationValidationService _paginationService;
        private readonly IDtoFactory<RecipeDto> _factory;
        private readonly IRecordQueryRepository<EdgeTypeSearchRequest, QueryAggregateModel> _edgeTypeRepository;
        private readonly IRecordQueryRepository<IdSearchRequest, InsertAggregateModel> _idRepository;

        public RecipeProvider(IPaginationValidationService paginationService,
            IDtoFactory<RecipeDto> factory,
        IRecordQueryRepository<EdgeTypeSearchRequest, QueryAggregateModel> edgeTypeRepository,
            IRecordQueryRepository<IdSearchRequest, InsertAggregateModel> idRepository)
        {
            _paginationService = paginationService;
            _factory = factory;
            _edgeTypeRepository = edgeTypeRepository;
            _idRepository = idRepository;
        }

        public async Task<PaginatedResult<List<RecipeDto>>> GetAsync(int? page, int? limit, CancellationToken cancellationToken)
        {
            EdgeTypeSearchRequest edgeTypeSearchRequest = new EdgeTypeSearchRequest()
            {
                EdgeType = (int)EdgeTypeEnum.Recipe,
                Active = true,
                FilterRequestType = EdgeRequestType.Edge | EdgeRequestType.RootNode,
                SelectReferences = EdgeRequestType.Edge | EdgeRequestType.TargetNode,
                SelectRecordTypes = RecordRequestType.Edges | RecordRequestType.TermEntries | RecordRequestType.FlexRecords,
                Pagination = new PaginationMetadata(_paginationService.GetValidPage(page), _paginationService.GetValidLimit(limit))
            };

            Result<QueryAggregateModel> result = await _edgeTypeRepository.FetchRecordsAsync(edgeTypeSearchRequest, cancellationToken).ConfigureAwait(false);
            if (result.IsError)
            {
                return result.Error;
            }
            return _factory.CreatePaged(result.Value);
        }

        public async Task<Result<RecipeDto>> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!BureauReferenceFactory.TryCreateReference(id, out IReference referenceId))
            {
                return RecipeResultErrorFactory.RecipeIdBadFormat(id);
            }

            Result<InsertAggregateModel> result = await InternalGetAggregateAsync(referenceId, cancellationToken).ConfigureAwait(false);

            if (result.IsError)
            {
                return result.Error;
            }
            return _factory.Create(result.Value);
        }

        public async Task<Result<InsertAggregateModel>> InternalGetAggregateAsync(IReference id, CancellationToken cancellationToken)
        {
            IdSearchRequest idSearchRequest = new IdSearchRequest()
            {
                FilterReferenceId = id,
                FilterRequestType = EdgeRequestType.RootNode,
                SelectReferences = EdgeRequestType.Edge | EdgeRequestType.TargetNode,
                SelectRecordTypes = RecordRequestType.Edges | RecordRequestType.TermEntries | RecordRequestType.FlexRecords,
            };

            return await _idRepository.FetchRecordsAsync(idSearchRequest, cancellationToken).ConfigureAwait(false);
        }
    }
}
