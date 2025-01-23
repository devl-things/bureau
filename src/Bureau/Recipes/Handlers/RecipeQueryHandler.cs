using Bureau.Core;
using Bureau.Core.Configuration;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Core.Services;
using Bureau.Models;
using Bureau.Recipes.Factories;
using Bureau.Recipes.Models;
using Microsoft.Extensions.Options;

namespace Bureau.Recipes.Handlers
{
    internal class RecipeQueryHandler : IRecipeQueryHandler, IInternalRecipeQueryHandler
    {
        private readonly IPaginationValidationService _paginationService;
        private readonly IRecordQueryRepository<EdgeTypeSearchRequest, QueryAggregateModel> _edgeTypeRepository;
        private readonly IRecordQueryRepository<IdSearchRequest, InsertAggregateModel> _idRepository;

        public RecipeQueryHandler(IPaginationValidationService paginationService,
            IRecordQueryRepository<EdgeTypeSearchRequest, QueryAggregateModel> edgeTypeRepository,
            IRecordQueryRepository<IdSearchRequest, InsertAggregateModel> idRepository)
        {
            _paginationService = paginationService;
            _edgeTypeRepository = edgeTypeRepository;
            _idRepository = idRepository;
        }

        public async Task<Result<RecipeDto>> GetRecipeAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!BureauReferenceFactory.TryCreateReference(id, out IReference referenceId))
            {
                return RecipeResultErrorFactory.RecipeIdBadFormat(id);
            }

            Result<InsertAggregateModel> result = await InternalGetRecipeAggregateAsync(referenceId, cancellationToken).ConfigureAwait(false);

            if (result.IsError)
            {
                return result.Error;
            }
            return RecipeDtoFactory.Create(result.Value);
        }

        public async Task<PaginatedResult<List<RecipeDto>>> GetRecipesAsync(int? page, int? limit, CancellationToken cancellationToken)
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
            return RecipeDtoFactory.CreatePaged(result.Value);
        }

        public async Task<Result<InsertAggregateModel>> InternalGetRecipeAggregateAsync(IReference id, CancellationToken cancellationToken)
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
