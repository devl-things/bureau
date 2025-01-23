using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Comparers;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Factories;
using Bureau.Models;

namespace Bureau.Calendar.Factories
{
    internal class CalendarBaseAggregateFactory : BaseAggregateFactory 
    {
        protected const int ExpectedNumberOfEdges = 1;
        protected readonly CalendarDto _calendar;
        public CalendarBaseAggregateFactory(CalendarDto calendar) : base(ExpectedNumberOfEdges)
        {
            _calendar = calendar;
            if (string.IsNullOrWhiteSpace(_calendar.Id))
            {
                _calendar.Id = GetNewTempId();
            }

            TermLabels.Add(TermEntry.GetLabel(_calendar.Name));
        }
        public override void UpdateTerms(Dictionary<string, TermEntry> value)
        {
            foreach (string label in TermLabels)
            {
                if (value.TryGetValue(label, out TermEntry? termEntry))
                {
                    termEntry.UpdatedAt = _calendar.UpdatedAt;
                }
                else
                {
                    termEntry = new TermEntry(GetNewTempId(), _calendar.Name)
                    {
                        CreatedAt = _calendar.CreatedAt,
                        UpdatedAt = _calendar.UpdatedAt,
                        Status = RecordStatus.Active,
                    };
                }
                TermEntriesByLabel.Add(label, termEntry);
            }
        }
    }

    internal class CalendarInsertAggregateFactory : CalendarBaseAggregateFactory, IAggregateFactory<InsertAggregateModel>
    {
        public CalendarInsertAggregateFactory(CalendarDto calendar) : base(calendar)
        {
        }

        public Result<InsertAggregateModel> CreateAggregate()
        {
            if (!BureauReferenceFactory.IsTempId(_calendar.Id))
            {
                return ResultErrorFactory.RecordIdBadFormat(nameof(CalendarDto), _calendar.Id);
            }

            if (!TryGetTermEntry(_calendar.Name, out TermEntry headerEntry))
            {
                return ResultErrorFactory.UnknownTerm(_calendar.Name);
            }

            Edge mainEdge = new Edge(_calendar.Id)
            {
                RootNode = BureauReferenceFactory.CreateReference(_calendar.Id),
                SourceNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                TargetNode = BureauReferenceFactory.CreateReference(headerEntry.Id),
                CreatedAt = _calendar.CreatedAt,
                UpdatedAt = _calendar.UpdatedAt,
                EdgeType = (int)EdgeTypeEnum.Calendar,
            };

            InsertAggregateModel newAggregate = new InsertAggregateModel()
            {
                MainReference = mainEdge,
                TermEntries = new HashSet<TermEntry>(TermEntriesByLabel.Values, new ReferenceComparer()),
                Edges = new HashSet<Edge>([mainEdge], new ReferenceComparer()),
            };
            if (!string.IsNullOrEmpty(_calendar.Description)) 
            {
                Result<FlexRecord> flexResult = FlexRecordFactory
                    .CreateFlexRecord(new FlexibleRecord<NoteDetails>(mainEdge.Id)
                    {
                        Data = new NoteDetails()
                        {
                            Note = _calendar.Description
                        },
                        CreatedAt = _calendar.CreatedAt,
                        UpdatedAt = _calendar.UpdatedAt,
                    });
                if (flexResult.IsError)
                {
                    return flexResult.Error;
                }
                newAggregate.FlexRecords = new HashSet<FlexRecord>([flexResult.Value], new ReferenceComparer());
            }
            return newAggregate;
        }
    }

    internal class CalendarUpdateAggregateFactory : CalendarBaseAggregateFactory, IAggregateFactory<UpdateAggregateModel>
    {
        private readonly InsertAggregateModel _existingCalendar;
        public CalendarUpdateAggregateFactory(CalendarDto calendar, InsertAggregateModel existingCalendar) : base(calendar)
        {
            _existingCalendar = existingCalendar;
        }
        public Result<UpdateAggregateModel> CreateAggregate()
        {
            Result validationResult = Validate();
            if (validationResult.IsError) 
            {
                return validationResult.Error;
            }

            Edge calendarEdge;
            if (!_existingCalendar.Edges.TryGetValue(Edge.EmptyEdgeWithId(_existingCalendar.MainReference.Id), out calendarEdge!))
            {
                return ResultErrorFactory.EdgeNotFound(_existingCalendar.MainReference.Id, nameof(Calendar));
            }

            if (!TryGetTermEntry(_calendar.Name, out TermEntry headerEntry))
            {
                return ResultErrorFactory.UnknownTerm(_calendar.Name);
            }
            calendarEdge.SourceNode = BureauReferenceFactory.CreateReference(headerEntry.Id);
            calendarEdge.TargetNode = BureauReferenceFactory.CreateReference(headerEntry.Id);
            calendarEdge.UpdatedAt = _calendar.UpdatedAt;

            _existingCalendar.FlexRecords.TryGetValue(new FlexRecord(calendarEdge.Id), out FlexRecord? existingDetails);
            FlexRecord? details = default;
            FlexRecord? toDeleteDetails = default!;
            if (!string.IsNullOrWhiteSpace(_calendar.Description))
            {
                Result<FlexRecord> flexResult = FlexRecordFactory
                    .CreateFlexRecord(new FlexibleRecord<NoteDetails>(calendarEdge.Id)
                    {
                        Data = new NoteDetails()
                        {
                            Note = _calendar.Description
                        },
                        CreatedAt = _calendar.CreatedAt,
                        UpdatedAt = _calendar.UpdatedAt,
                    });
                if (flexResult.IsError)
                {
                    return flexResult.Error;
                }

                if (existingDetails != null)
                {
                    existingDetails!.UpdatedAt = _calendar.UpdatedAt;
                    existingDetails!.DataType = flexResult.Value.DataType;
                    existingDetails!.Data = flexResult.Value.Data;
                    details = existingDetails;
                }
                else
                {
                    details = flexResult.Value;
                }
            }
            else
            {
                if (existingDetails != null)
                {
                    toDeleteDetails = existingDetails;
                }
            }

            UpdateAggregateModel updateRecipe = new UpdateAggregateModel()
            {
                MainReference = calendarEdge,
                TermEntries = new HashSet<TermEntry>(TermEntriesByLabel.Values, new ReferenceComparer()),
                Edges = new HashSet<Edge>([calendarEdge], new ReferenceComparer()),
                FlexRecords = details == null ? new HashSet<FlexRecord>(0) : new HashSet<FlexRecord>([details], new ReferenceComparer()),
                EdgesToDelete = new HashSet<Edge>(0),
                FlexRecordsToDelete = toDeleteDetails == null ? new HashSet<FlexRecord>(0) : new HashSet<FlexRecord>([toDeleteDetails], new ReferenceComparer())
            };

            return updateRecipe;

        }

        private Result Validate()
        {
            if (BureauReferenceFactory.IsTempId(_calendar.Id))
            {
                return ResultErrorFactory.RecordIdBadFormat(nameof(CalendarDto), _calendar.Id);
            }
            if (!BureauReferenceFactory.CreateReference(_calendar.Id).Equals(_existingCalendar.MainReference))
            {
                return ResultErrorFactory.EdgeNotFound(_calendar.Id, nameof(Calendar));
            }
            if (_existingCalendar.Edges.Count != ExpectedNumberOfEdges)
            {
                return ResultErrorFactory.UnexpectedNumberOrEdges(ExpectedNumberOfEdges, _existingCalendar.Edges.Count);
            }
            return true;
        }
    }
}
