using Bureau.Calendar.Factories;
using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Comparers;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Handlers;
using Bureau.Recipes.Factories;
using Bureau.Recipes.Models;
using System.Reflection.Metadata;
using System.Threading;

namespace Bureau.Calendar.Services
{
    internal class CalendarCommandHandler : ICommandHandler<CalendarDto>
    {
        private readonly IRecordCommandRepository _repository;
        private readonly ITermRepository _termRepository;
        private readonly IInternalCalendarQueryHandler _queryHandler;
        public CalendarCommandHandler(IRecordCommandRepository repository,
            ITermRepository termRepository,
            IInternalCalendarQueryHandler queryHandler)
        {
            _repository = repository;
            _termRepository = termRepository;
            _queryHandler = queryHandler;
        }
        public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            if (BureauReferenceFactory.IsTempId(id))
            {
                return ResultErrorFactory.RecordIdBadFormat(nameof(Calendar), id);
            }
            Result<InsertAggregateModel> existingCalendarResult = await _queryHandler.InternalGetAggregateAsync(BureauReferenceFactory.CreateReference(id), cancellationToken).ConfigureAwait(false);
            if (existingCalendarResult.IsError)
            {
                return existingCalendarResult.Error;
            }
            RemoveAggregateModel removeRecipe = new RemoveAggregateModel()
            {
                EdgesToDelete = new HashSet<Edge>(existingCalendarResult.Value.Edges.Count, new ReferenceComparer()),
                FlexRecordsToDelete = new HashSet<FlexRecord>(existingCalendarResult.Value.FlexRecords.Count, new ReferenceComparer())
            };
            foreach (Edge edge in existingCalendarResult.Value.Edges)
            {
                removeRecipe.EdgesToDelete.Add(edge);
                if (existingCalendarResult.Value.FlexRecords.TryGetValue(new FlexRecord(edge.Id), out FlexRecord? flexRecord))
                {
                    removeRecipe.FlexRecordsToDelete.Add(flexRecord);
                }
            }

            return await _repository.DeleteAggregateAsync(removeRecipe, cancellationToken);
        }

        public async Task<Result<IReference>> InsertAsync(CalendarDto dto, CancellationToken cancellationToken)
        {
            CalendarInsertAggregateFactory factory = new CalendarInsertAggregateFactory(dto);

            Result<Dictionary<string, TermEntry>> existingTermsResult = await FetchTermEntriesAsync(factory.TermLabels, cancellationToken);
            if (existingTermsResult.IsError)
            {
                return existingTermsResult.Error;
            }
            factory.UpdateTerms(existingTermsResult.Value);

            Result<InsertAggregateModel> newAggregateResult = factory.CreateAggregate();
            if (newAggregateResult.IsError) 
            {
                return newAggregateResult.Error;
            }

            return await _repository.InsertAggregateAsync(newAggregateResult.Value, cancellationToken);
        }

        private Task<Result<Dictionary<string, TermEntry>>> FetchTermEntriesAsync(HashSet<string> termLabels, CancellationToken cancellationToken) 
        {
            TermSearchRequest termSearchRequest = new TermSearchRequest()
            {
                Terms = termLabels,
                RequestType = TermRequestType.Label
            };

            return _termRepository.FetchTermRecordsAsync(termSearchRequest, cancellationToken);
        }

        public async Task<Result<IReference>> UpdateAsync(CalendarDto dto, CancellationToken cancellationToken)
        {
            Result<InsertAggregateModel> existingCalendarResult = await _queryHandler.InternalGetAggregateAsync(BureauReferenceFactory.CreateReference(dto.Id), cancellationToken).ConfigureAwait(false);
            if (existingCalendarResult.IsError)
            {
                return existingCalendarResult.Error;
            }
            CalendarUpdateAggregateFactory factory = new CalendarUpdateAggregateFactory(dto, existingCalendarResult.Value);

            Result<Dictionary<string, TermEntry>> existingTermsResult = await FetchTermEntriesAsync(factory.TermLabels, cancellationToken);
            if (existingTermsResult.IsError)
            {
                return existingTermsResult.Error;
            }
            factory.UpdateTerms(existingTermsResult.Value);

            Result<UpdateAggregateModel> updateAggregateResult = factory.CreateAggregate();
            if (updateAggregateResult.IsError)
            {
                return updateAggregateResult.Error;
            }

            return await _repository.UpdateAggregateAsync(updateAggregateResult.Value, cancellationToken);
        }
    }
}
