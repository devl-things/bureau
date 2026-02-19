using Bureau;
using Bureau.Primitives.Errors;
using Microsoft.EntityFrameworkCore;
using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Contexts;
using Watson.Nodes.Mappers;
using Watson.Nodes.Models;

namespace Watson.Nodes.Services
{
    internal sealed class NodeEdgeService : INodeEdgeService
    {
        private readonly NodesContext _context;

        public NodeEdgeService(NodesContext context)
        {
            _context = context;
        }

        public async Task<Result<NodeEdge>> AddEdgeAsync(Guid sourceNodeId, CreateEdgeCommand command, CancellationToken cancellationToken = default)
        {
            Result purposeValidation = EdgeConventions.Validate(command.Purpose);
            if (purposeValidation.IsError)
            {
                return purposeValidation.Error;
            }

            Result differentValidation = EdgeConventions.ValidateSourceTargetDifferent(sourceNodeId, command.TargetNodeId);
            if (differentValidation.IsError)
            {
                return differentValidation.Error;
            }

            //TODO reduce number of queries fetching both source and target nodes together
            bool sourceExists = await _context.Nodes.AsNoTracking().AnyAsync(x => x.NodeId == sourceNodeId, cancellationToken);
            if (!sourceExists)
            {
                return ResultError.From(ProblemCodes.Resource.NotFound, $"Source node {sourceNodeId} not found.");
            }

            bool targetExists = await _context.Nodes.AsNoTracking().AnyAsync(x => x.NodeId == command.TargetNodeId, cancellationToken);
            if (!targetExists)
            {
                return ResultError.From(ProblemCodes.Resource.NotFound, $"Target node {command.TargetNodeId} not found.");
            }

            bool alreadyExists = await _context.NodeEdges.AsNoTracking()
                .AnyAsync(x => x.SourceNodeId == sourceNodeId && x.TargetNodeId == command.TargetNodeId && x.Purpose == command.Purpose, cancellationToken);
            if (alreadyExists)
            {
                return ResultError.From(ProblemCodes.Resource.Conflict, "Edge already exists.");
            }

            NodeEdgeDb edgeDb = new NodeEdgeDb
            {
                SourceNodeId = sourceNodeId,
                TargetNodeId = command.TargetNodeId,
                Purpose = command.Purpose!,
                OrderIndex = command.OrderIndex
            };

            await _context.NodeEdges.AddAsync(edgeDb, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return edgeDb.ToDomain();
        }

        public async Task<Result> RemoveEdgeAsync(Guid sourceNodeId, RemoveEdgeCommand command, CancellationToken cancellationToken = default)
        {
            Result purposeValidation = EdgeConventions.Validate(command.Purpose);
            if (purposeValidation.IsError)
            {
                return purposeValidation.Error;
            }

            NodeEdgeDb? edgeDb = await _context.NodeEdges
                .FirstOrDefaultAsync(x => x.SourceNodeId == sourceNodeId && x.TargetNodeId == command.TargetNodeId && x.Purpose == command.Purpose, cancellationToken);

            if (edgeDb == null)
            {
                return ResultError.From(ProblemCodes.Resource.NotFound, "Edge not found.");
            }

            _context.NodeEdges.Remove(edgeDb);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<CursorResult<NodeEdge>> GetEdgesAsync(Guid sourceNodeId, SearchEdgesQuery query, CancellationToken cancellationToken = default)
        {
            bool sourceExists = await _context.Nodes.AsNoTracking().AnyAsync(x => x.NodeId == sourceNodeId, cancellationToken);
            if (!sourceExists)
            {
                return ResultError.From(ProblemCodes.Resource.NotFound, $"Source node {sourceNodeId} not found.");
            }

            IQueryable<NodeEdgeDb> baseQuery = _context.NodeEdges.AsNoTracking()
                .Where(x => x.SourceNodeId == sourceNodeId);

            if (!string.IsNullOrWhiteSpace(query.Purpose))
            {
                baseQuery = baseQuery.Where(x => x.Purpose == query.Purpose);
            }

            int limit = query.Cursor.Limit;
            long cursor = query.Cursor.Cursor;

            //TODO this is not good, what's .Skip((int)cursor)
            List<NodeEdgeDb> page = await baseQuery
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.TargetNodeId)
                .Skip((int)cursor)
                .Take(limit + 1)
                .ToListAsync(cancellationToken);

            bool hasMore = page.Count > limit;
            if (hasMore)
            {
                page.RemoveAt(page.Count - 1);
            }

            long nextCursor = cursor + page.Count;

            List<NodeEdge> edges = page.Select(x => x.ToDomain()).ToList();

            return new CursorResult<NodeEdge>(edges, query.Cursor, nextCursor, hasMore);
        }
    }
}
