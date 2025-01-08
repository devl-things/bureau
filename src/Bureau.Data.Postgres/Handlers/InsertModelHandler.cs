using Bureau.Core.Factories;
using Bureau.Core.Models.Data;
using Bureau.Core.Models;
using Bureau.Core;
using Bureau.Data.Postgres.Mappers;
using dbModels = Bureau.Data.Postgres.Models;
using Bureau.Data.Postgres.Models;

namespace Bureau.Data.Postgres.Handlers
{
    internal interface IModelHandler 
    {
        public Result HandleAggregate();
    }
    internal class BaseModelHandler
    {
        protected Dictionary<IReference, Guid> _newRecordIds;

        public BaseModelHandler(int capacity)
        {
            _newRecordIds = new Dictionary<IReference, Guid>(capacity, new Core.Comparers.ReferenceComparer());
        }
        public BaseModelHandler() : this(0)
        {
        }
        protected bool TryGetGuid(IReference reference, out Guid guid)
        {
            if (BureauReferenceFactory.IsTempId(reference.Id))
            {
                if (_newRecordIds.TryGetValue(reference, out Guid tempId))
                {
                    guid = tempId;
                    return true;
                }
                guid = Guid.Empty;
                return false;
            }
            else
            {
                return Guid.TryParse(reference.Id, out guid);
            }
        }
    }
    internal class InsertModelHandler : BaseModelHandler, IModelHandler
    {
        private InsertAggregateModel _insertRequest;

        private DateTime _updatedAt = default;
        private List<IReference> _updateRecords;
        private List<Record> _newRecords;
        
        private List<dbModels.TermEntry> _newTermEntries;
        private List<dbModels.Edge> _newEdgeRecords;
        private List<dbModels.Edge> _updateEdgeRecords;
        private List<FlexibleRecord> _newFlexibleRecords;
        private List<FlexibleRecord> _updateFlexibleRecords;
        private IReference _mainReference = BureauReferenceFactory.EmptyReference;
        public InsertModelHandler(InsertAggregateModel insertRequest) : base(insertRequest.TermEntries.Count + insertRequest.Edges.Count)
        {
            _insertRequest = insertRequest;
            _updateRecords = new List<IReference>(_insertRequest.TermEntries.Count + _insertRequest.Edges.Count);
            _newRecords = new List<Record>(_insertRequest.TermEntries.Count + _insertRequest.Edges.Count);
            _newTermEntries = new List<dbModels.TermEntry>(_insertRequest.TermEntries.Count);
            _newEdgeRecords = new List<dbModels.Edge>(_insertRequest.Edges.Count);
            _updateEdgeRecords = new List<dbModels.Edge>(_insertRequest.Edges.Count);
            _newFlexibleRecords = new List<FlexibleRecord>(_insertRequest.FlexRecords.Count);
            _updateFlexibleRecords = new List<FlexibleRecord>(_insertRequest.FlexRecords.Count);
        }

        public IReference MainReference
        {
            get { return _mainReference; }
        }
        /// <summary>
        /// There shouldn't be duplicates, but!s
        /// </summary>
        public List<Record> NewRecords { get { return _newRecords; } }

        public List<IReference> UpdateRecords { get { return _updateRecords; } }

        public List<dbModels.TermEntry> NewTermEntries { get { return _newTermEntries; } }

        public List<dbModels.Edge> NewEdgeRecords { get { return _newEdgeRecords; } }
        public List<dbModels.Edge> UpdateEdgeRecords { get { return _updateEdgeRecords; } }

        public List<dbModels.FlexibleRecord> NewFlexibleRecords { get { return _newFlexibleRecords; } }
        public List<dbModels.FlexibleRecord> UpdateFlexibleRecords { get { return _updateFlexibleRecords; } }

        private void SetUpdatedAt(DateTime updatedAt)
        {
            if (_updatedAt == default)
            {
                _updatedAt = updatedAt;
            }
        }

        public virtual Result HandleAggregate()
        {
            ProcessTermEntries();
            Result result = ProcessEdges();
            if (result.IsError)
            {
                return result.Error;
            }
            result = HandleFlexibleRecords();
            if (result.IsError)
            {
                return result.Error;
            }
            if (TryGetGuid(_insertRequest.MainReference, out Guid guid))
            {
                _mainReference = BureauReferenceFactory.CreateReference(guid.ToString());
            }
            return true;
        }

        private Result HandleFlexibleRecords()
        {
            foreach (FlexRecord item in _insertRequest.FlexRecords)
            {
                FlexibleRecord flexRecord;
                if (TryGetGuid(item, out Guid guid))
                {
                    flexRecord = item.ToDBFlexibleRecord(guid);
                }
                else
                {
                    return "FlexRecord is missing required references.";
                }
                if (BureauReferenceFactory.IsTempId(item.Id))
                {
                    _newFlexibleRecords.Add(flexRecord);
                }
                else
                {
                    _updateFlexibleRecords.Add(flexRecord);
                }
            }
            return true;
        }

        private Result ProcessEdges()
        {
            foreach (Core.Models.Edge edge in _insertRequest.Edges)
            {
                if (BureauReferenceFactory.IsTempId(edge.Id))
                {
                    Guid guid = Guid.NewGuid();
                    _newRecordIds.Add(edge, guid);
                    _newRecords.Add(edge.ToRecord(guid));
                }
                else
                {
                    _updateRecords.Add(edge);
                    SetUpdatedAt(edge.UpdatedAt);
                }
            }

            foreach (Core.Models.Edge edge in _insertRequest.Edges)
            {
                Result<dbModels.Edge> edgeEntryResult = CreateEdge(edge);
                if (edgeEntryResult.IsError)
                {
                    return edgeEntryResult.Error;
                }
                if (BureauReferenceFactory.IsTempId(edge.Id))
                {
                    _newEdgeRecords.Add(edgeEntryResult.Value);
                }
                else
                {
                    _updateEdgeRecords.Add(edgeEntryResult.Value);
                }
            }
            return true;
        }

        private void ProcessTermEntries()
        {
            foreach (Core.Models.TermEntry term in _insertRequest.TermEntries)
            {
                if (BureauReferenceFactory.IsTempId(term.Id))
                {
                    Guid guid = Guid.NewGuid();
                    _newRecordIds.Add(term, guid);
                    dbModels.TermEntry termEntry = term.ToDBTermEntry(guid);
                    _newTermEntries.Add(termEntry);
                    _newRecords.Add(termEntry.Record);
                }
                else
                {
                    _updateRecords.Add(term);
                    SetUpdatedAt(term.UpdatedAt);
                }
            }
        }

        private Result<dbModels.Edge> CreateEdge(Core.Models.Edge edge)
        {
            bool idExists = TryGetGuid(edge, out Guid id);
            bool sourceNodeIdExists = TryGetGuid(edge.SourceNode, out Guid sourceNodeId);
            bool targetNodeIdExists = TryGetGuid(edge.TargetNode, out Guid targetNodeId);
            Guid? parentNodeId = null;
            if (edge.ParentNode != null && TryGetGuid(edge.ParentNode, out Guid tempId))
            {
                parentNodeId = tempId;
            }
            bool rootExists = TryGetGuid(edge.RootNode, out Guid rootNodeId);

            if (!idExists || !sourceNodeIdExists || !targetNodeIdExists || !rootExists)
            {
                return "Edge is missing required references.";
            }

            dbModels.Edge edgeEntry = new dbModels.Edge()
            {
                Id = id,
                SourceNodeId = sourceNodeId,
                TargetNodeId = targetNodeId,
                EdgeType = edge.EdgeType,
                Active = edge.Active,
                ParentNodeId = parentNodeId,
                RootNodeId = rootNodeId,
                Order = edge.Order,
            };

            return edgeEntry;
        }

        
    }

    internal class UpdateModelHandler : InsertModelHandler
    {
        private RemoveModelHandler _deleteHandler;
        public UpdateModelHandler(UpdateAggregateModel insertRequest) : base(insertRequest)
        {
            _deleteHandler = new RemoveModelHandler(insertRequest);
        }

        public HashSet<Guid> RemoveRecords { get { return _deleteHandler.RemoveRecords; } }
        public HashSet<Guid> RemoveEdgeRecords { get { return _deleteHandler.RemoveEdgeRecords; } }
        public HashSet<Guid> RemoveFlexibleRecords { get { return _deleteHandler.RemoveFlexibleRecords; } }

        public override Result HandleAggregate()
        {
            Result result = base.HandleAggregate();
            if (result.IsError) 
            {
                return result.Error;
            }

            return _deleteHandler.HandleAggregate();
        }
    }

    internal class RemoveModelHandler : IModelHandler
    {
        private IRemoveAggregateModel _request;
        private HashSet<Guid> _removeRecordIds;
        private HashSet<Guid> _removeFlexIds;
        private HashSet<Guid> _removeEdgeIds;
        public RemoveModelHandler(IRemoveAggregateModel request)
        {
            _request = request;
            _removeRecordIds = new HashSet<Guid>(request.EdgesToDelete.Count);
            _removeFlexIds = new HashSet<Guid>(request.FlexRecordsToDelete.Count);
            _removeEdgeIds = new HashSet<Guid>(request.EdgesToDelete.Count);
        }

        public HashSet<Guid> RemoveRecords { get { return _removeRecordIds; } }
        public HashSet<Guid> RemoveEdgeRecords { get { return _removeEdgeIds; } }
        public HashSet<Guid> RemoveFlexibleRecords { get { return _removeFlexIds; } }

        public Result HandleAggregate()
        {
            foreach (IReference item in _request.FlexRecordsToDelete)
            {
                if (Guid.TryParse(item.Id, out Guid guid))
                {
                    _removeFlexIds.Add(guid);
                }
            }

            foreach (IReference item in _request.EdgesToDelete)
            {
                if (Guid.TryParse(item.Id, out Guid guid))
                {
                    _removeEdgeIds.Add(guid);
                    _removeRecordIds.Add(guid);
                }
            }
            return true;
        }
    }
}
