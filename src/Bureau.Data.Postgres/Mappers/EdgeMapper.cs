using Bureau.Core.Factories;
using Bureau.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Data.Postgres.Mappers
{
    internal static class EdgeMapper
    {
        internal static Core.Models.Edge ToModel(this Postgres.Models.Edge edge)
        {
            return new Core.Models.Edge(edge.Id.ToString())
            {
                SourceNode = BureauReferenceFactory.CreateReference(edge.SourceNodeId.ToString()),
                TargetNode = BureauReferenceFactory.CreateReference(edge.TargetNodeId.ToString()),
                RootNode = BureauReferenceFactory.CreateReference(edge.RootNodeId.ToString()),
                ParentNode = edge.ParentNodeId == null ? null : BureauReferenceFactory.CreateReference(edge.ParentNodeId.ToString()!),
                Order = edge.Order,
                EdgeType = edge.EdgeType,
                Active = edge.Active,
                CreatedAt = edge.Record.CreatedAt,
                UpdatedAt = edge.Record.UpdatedAt,
                CreatedBy = edge.Record.CreatedBy,
                UpdatedBy = edge.Record.UpdatedBy,
                Status = (RecordStatus)edge.Record.Status
            };
        }
    }
}
