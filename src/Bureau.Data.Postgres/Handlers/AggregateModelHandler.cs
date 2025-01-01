using Bureau.Core.Factories;
using Bureau.Core.Models.Data;
using Bureau.Core.Models;
using Bureau.Core;
using Bureau.Data.Postgres.Mappers;
using dbModels = Bureau.Data.Postgres.Models;
using System.Runtime.CompilerServices;
using Bureau.Data.Postgres.Models;
using Npgsql;
using System.Text;
using Bureau.Core.Comparers;

namespace Bureau.Data.Postgres.Handlers
{
    internal class AggregateModelHandler
    {
        private AggregateModel _insertRequest;

        private DateTime _updatedAt = default;
        private List<IReference> _updateRecords;
        private List<Record> _newRecords;
        private Dictionary<IReference, Guid> _newRecordIds;
        private List<dbModels.TermEntry> _newTermEntries;
        private List<dbModels.Edge> _newEdgeRecords;
        private List<dbModels.Edge> _updateEdgeRecords;
        private List<FlexibleRecord> _newFlexibleRecords;
        private List<FlexibleRecord> _updateFlexibleRecords;
        private IReference _mainReference = BureauReferenceFactory.EmptyReference;
        public AggregateModelHandler(AggregateModel insertRequest)
        {
            _insertRequest = insertRequest;
            _updateRecords = new List<IReference>(_insertRequest.TermEntries.Count + _insertRequest.Edges.Count);
            _newRecordIds = new Dictionary<IReference, Guid>(_insertRequest.TermEntries.Count + _insertRequest.Edges.Count, new Core.Comparers.ReferenceComparer());
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

    internal class ExtendedAggregateModelHandler : AggregateModelHandler
    {
        private ExtendedAggregateModel _insertRequest;
        private HashSet<Guid> _removeRecordIds;
        private HashSet<Guid> _removeFlexIds;
        private HashSet<Guid> _removeEdgeIds;
        public ExtendedAggregateModelHandler(ExtendedAggregateModel insertRequest) : base(insertRequest)
        {
            _insertRequest = insertRequest;
            _removeRecordIds = new HashSet<Guid>(insertRequest.FlexRecordsToDelete.Count + insertRequest.EdgesToDelete.Count);
            _removeFlexIds = new HashSet<Guid>(insertRequest.FlexRecordsToDelete.Count);
            _removeEdgeIds = new HashSet<Guid>(insertRequest.EdgesToDelete.Count);
        }

        public HashSet<Guid> RemoveRecords { get { return _removeRecordIds; } }
        public HashSet<Guid> RemoveEdgeRecords { get { return _removeEdgeIds; } }
        public HashSet<Guid> RemoveFlexibleRecords { get { return _removeFlexIds; } }

        public override Result HandleAggregate()
        {
            base.HandleAggregate();

            foreach (IReference item in _insertRequest.FlexRecordsToDelete)
            {
                if (TryGetGuid(item, out Guid guid))
                {
                    _removeFlexIds.Add(guid);
                }
            }

            foreach (IReference item in _insertRequest.EdgesToDelete)
            {
                if (TryGetGuid(item, out Guid guid))
                {
                    _removeEdgeIds.Add(guid);
                    _removeRecordIds.Add(guid);
                }
            }
            return true;
        }
    }
}
