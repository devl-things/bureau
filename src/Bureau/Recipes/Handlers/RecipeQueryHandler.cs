using Bureau.Core;
using Bureau.Core.Comparers;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Models;
using Bureau.Recipes.Abstractions.Factories;
using Bureau.Recipes.Factories;
using Bureau.Recipes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Recipes.Handlers
{
    internal class RecipeQueryHandler : IRecipeQueryHandler, IInternalRecipeQueryHandler
    {
        private readonly IRecordQueryRepository<EdgeTypeSearchRequest, BaseAggregateModel> _edgeTypeRepository;
        private readonly IRecordQueryRepository<IdSearchRequest, AggregateModel> _idRepository;

        public RecipeQueryHandler(IRecordQueryRepository<EdgeTypeSearchRequest, BaseAggregateModel> edgeTypeRepository,
            IRecordQueryRepository<IdSearchRequest, AggregateModel> idRepository)
        {
            _edgeTypeRepository = edgeTypeRepository;
            _idRepository = idRepository;
        }
        public async Task<Result<RecipeDto>> GetRecipeAsync(string id, CancellationToken cancellationToken = default)
        {
            if(!BureauReferenceFactory.TryCreateReference(id, out IReference referenceId))
            {
                return RecipeResultErrorFactory.RecipeIdBadFormat(id);
            }

            Result<AggregateModel> result = await InternalGetRecipeAggregateAsync(referenceId, cancellationToken).ConfigureAwait(false);

            if (result.IsError)
            {
                return result.Error;
            }
            return RecipeDtoFactory.Create(result.Value);
        }

        public async Task<PaginatedResult<List<RecipeDto>>> GetRecipesAsync(CancellationToken cancellationToken)
        {
            EdgeTypeSearchRequest edgeTypeSearchRequest = new EdgeTypeSearchRequest()
            {
                EdgeType = (int)EdgeTypeEnum.Recipe,
                Active = true,
                FilterRequestType = EdgeRequestType.Edge | EdgeRequestType.RootNode,
                SelectReferences = EdgeRequestType.Edge | EdgeRequestType.TargetNode,
                SelectRecordTypes = RecordRequestType.Edges | RecordRequestType.TermEntries | RecordRequestType.FlexRecords,
            };

            Result<BaseAggregateModel> result = await _edgeTypeRepository.FetchRecordsAsync(edgeTypeSearchRequest, cancellationToken).ConfigureAwait(false);
            if (result.IsError)
            {
                return result.Error;
            }
            return RecipeDtoFactory.CreatePaged(result.Value);
        }

        public async Task<Result<AggregateModel>> InternalGetRecipeAggregateAsync(IReference id, CancellationToken cancellationToken)
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
