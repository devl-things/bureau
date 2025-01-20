using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Factories;
using Bureau.Core.Models.Data;
using Bureau.Core.Models;
using Bureau.Factories;
using Bureau.Core.Repositories;
using Bureau.Models;
using Bureau.Core.Services;
using Bureau.Providers;

namespace Bureau.Calendar.Services
{
    internal class CalendarProvider : IDtoProvider<CalendarDto>
    {
        private readonly IPaginationValidationService _paginationService;
        private readonly IDtoFactory<CalendarDto> _factory;
        private readonly IRecordQueryRepository<EdgeTypeSearchRequest, QueryAggregateModel> _edgeTypeRepository;
        private readonly IRecordQueryRepository<IdSearchRequest, InsertAggregateModel> _idRepository;
        public CalendarProvider(IPaginationValidationService paginationService,
            IDtoFactory<CalendarDto> factory,
            IRecordQueryRepository<EdgeTypeSearchRequest, QueryAggregateModel> edgeTypeRepository,
            IRecordQueryRepository<IdSearchRequest, InsertAggregateModel> idRepository)
        {
            _paginationService = paginationService;
            _factory = factory;
            _edgeTypeRepository = edgeTypeRepository;
            _idRepository = idRepository;
        }

        private async Task<Result<InsertAggregateModel>> InternalGetCalendarAggregateAsync(IReference referenceId, CancellationToken cancellationToken)
        {
            IdSearchRequest idSearchRequest = new IdSearchRequest()
            {
                FilterReferenceId = referenceId,
                FilterRequestType = EdgeRequestType.Edge,
                SelectReferences = EdgeRequestType.Edge,
                SelectRecordTypes = RecordRequestType.Edges | RecordRequestType.TermEntries | RecordRequestType.FlexRecords,
            };

            return await _idRepository.FetchRecordsAsync(idSearchRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Result<CalendarDto>> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!BureauReferenceFactory.TryCreateReference(id, out IReference referenceId))
            {
                return ResultErrorFactory.RecordIdBadFormat(nameof(Calendar), id);
            }

            Result<InsertAggregateModel> result = await InternalGetCalendarAggregateAsync(referenceId, cancellationToken).ConfigureAwait(false);

            if (result.IsError)
            {
                return result.Error;
            }
            return _factory.Create(result.Value);
        }

        public async Task<PaginatedResult<List<CalendarDto>>> GetAsync(int? page, int? limit, CancellationToken cancellationToken)
        {
            EdgeTypeSearchRequest edgeTypeSearchRequest = new EdgeTypeSearchRequest()
            {
                EdgeType = (int)EdgeTypeEnum.Calendar,
                Active = true,
                FilterRequestType = EdgeRequestType.Edge,
                SelectReferences = EdgeRequestType.Edge,
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
    }
}