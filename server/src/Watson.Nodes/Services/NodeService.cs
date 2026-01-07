using Bureau;
using Bureau.Primitives.Errors;
using Microsoft.EntityFrameworkCore;
using Watson.Nodes.Constants;
using Watson.Nodes.Contexts;
using Watson.Nodes.Mappers;
using Watson.Nodes.Models;

namespace Watson.Nodes.Services
{
    internal sealed class NodeService : INodeService
    {
        private readonly NodesContext _context;
        private readonly IReadOnlyDictionary<NodeKind, INodeKindHandler> _handlers;
        private readonly INodeKindHandler _fallbackHandler;

        public NodeService(NodesContext dbContext, IEnumerable<INodeKindHandler> handlers)
        {
            _context = dbContext;

            Dictionary<NodeKind, INodeKindHandler> map = new Dictionary<NodeKind, INodeKindHandler>();
            foreach (INodeKindHandler handler in handlers)
            {
                map[handler.Kind] = handler;
            }

            _handlers = map;

            if (_handlers.TryGetValue(NodeKind.None, out INodeKindHandler? fallback))
            {
                _fallbackHandler = fallback;
            }
            else
            {
                _fallbackHandler = new DefaultNodeKindHandler();
            }
        }

        private INodeKindHandler GetHandler(NodeKind kind)
        {
            if (_handlers.TryGetValue(kind, out INodeKindHandler? handler))
            {
                return handler;
            }

            return _fallbackHandler;
        }

        public async Task<Result<Node>> CreateAsync(CreateNodeCommand command, CancellationToken cancellationToken = default)
        {
            INodeKindHandler handler = GetHandler(command.Kind);

            Result validation = handler.ValidateCreate(command);
            if (validation.IsError)
            {
                return validation.Error;
            }
            //TODO add check for existing canonical key?
            Guid nodeId = Guid.NewGuid();

            NodeDb nodeDb = new NodeDb
            {
                NodeId = nodeId,
                Kind = command.Kind,
                Scope = command.Scope ?? Scopes.Global,
                CanonicalKey = command.CanonicalKey,
                Status = NodeStatus.Active,
                Version = 1
            };

            List<NodeAttributeDb> attributeDbs = new List<NodeAttributeDb>();
            foreach (NodeAttribute attribute in command.Attributes)
            {
                NodeAttributeDb attributeDb = attribute.ToDb(nodeId);
                attributeDbs.Add(attributeDb);
            }

            await _context.Nodes.AddAsync(nodeDb, cancellationToken);
            await _context.NodeAttributes.AddRangeAsync(attributeDbs, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            Node created = await LoadNodeAsync(nodeId, cancellationToken);

            Result after = await handler.AfterCreateAsync(created, command, cancellationToken);
            if (after.IsError)
            {
                return after.Error;
            }

            return created;
        }

        public async Task<Result<Node>> PatchNodeAttributesAsync(Guid nodeId, PatchNodeAttributesCommand attributeChanges, CancellationToken cancellationToken = default)
        {
            NodeDb? nodeDb = await _context.Nodes.FirstOrDefaultAsync(x => x.NodeId == nodeId, cancellationToken);

            if (nodeDb == null)
            {
                return ResultError.From(ProblemCodes.Resource.NotFound, string.Format("Not found, {0} with identifier {1}", nameof(Node), nodeId));
            }

            INodeKindHandler handler = GetHandler(nodeDb.Kind);

            Result validation = handler.ValidatePatch(nodeId, attributeChanges);
            if (validation.IsError)
            {
                return validation.Error;
            }

            List<NodeAttributeDb> existing = await _context.NodeAttributes
                .Where(x => x.NodeId == nodeId)
                .ToListAsync(cancellationToken);

            ApplyPatch(nodeId, existing, attributeChanges);

            nodeDb.Version = nodeDb.Version + 1;

            await _context.SaveChangesAsync(cancellationToken);

            Node updated = await LoadNodeAsync(nodeId, cancellationToken);
            return updated;
        }

        public async Task<Result<Node>> GetAsync(Guid nodeId, CancellationToken cancellationToken = default)
        {
            bool exists = await _context.Nodes.AsNoTracking().AnyAsync(x => x.NodeId == nodeId, cancellationToken);

            if (!exists)
            {
                return ResultError.From(ProblemCodes.Resource.NotFound, string.Format("Not found, {0} with identifier {1}", nameof(Node), nodeId));
            }

            Node node = await LoadNodeAsync(nodeId, cancellationToken);
            return node;
        }

        //TODO check from here on
        public async Task<CursorResult<Node>> SearchAsync(SearchNodesQuery query, CancellationToken cancellationToken = default)
        {
            SearchNodesFilter filter = query.Filter;

            INodeKindHandler handler = GetHandler(filter.Kind);

            CursorParameters cursor = query.Cursor;
            cursor.SetLimit(cursor.Limit);

            // Determine which attributes are searched (matching)
            IReadOnlyList<NodeAttributeKey> searchKeys = ResolveQueryKeys(filter, handler);

            // Determine which attributes are returned (projection)
            IReadOnlyList<NodeAttributeKey> projectionKeys = ResolveProjectionKeys(filter, handler);

            long after = cursor.Cursor;
            int limit = cursor.Limit;

            IQueryable<NodeDb> baseQuery = _context.Nodes.AsNoTracking()
                .Where(x => x.Kind == filter.Kind)
                .Where(x => filter.Scope == null || x.Scope == filter.Scope)
                .Where(x => x.CreatedSequence > after);

            if (!string.IsNullOrWhiteSpace(filter.Query))
            {
                string q = filter.Query.Trim();

                List<string> searchKeyStrings = searchKeys.Select(x => x.Key).Distinct().ToList();

                IQueryable<Guid> matched =
                    _context.NodeAttributes.AsNoTracking()
                        .Where(a => a.ValueString != null)
                        .Where(a => searchKeyStrings.Contains(a.Key))
                        .Where(a => filter.Locale == null || a.Locale == filter.Locale || a.Locale == null)
                        .Where(a => EF.Functions.Like(a.ValueString!, "%" + q + "%"))
                        .Select(a => a.NodeId);

                baseQuery = baseQuery.Where(n => matched.Contains(n.NodeId));
            }

            // Limit+1 pattern for HasMore
            List<NodeDb> page = await baseQuery
                .OrderBy(x => x.CreatedSequence)
                .Take(limit + 1)
                .ToListAsync(cancellationToken);

            bool hasMore = page.Count > limit;
            if (hasMore)
            {
                page = page.Take(limit).ToList();
            }

            long nextCursor = after;
            if (page.Count > 0)
            {
                nextCursor = page[page.Count - 1].CreatedSequence;
            }

            List<Guid> nodeIds = page.Select(x => x.NodeId).ToList();

            List<NodeAttributeDb> attrs = await LoadProjectedAttributesAsync(nodeIds, projectionKeys, filter.Locale, cancellationToken);

            List<Node> resultNodes = new List<Node>();
            foreach (NodeDb nodeDb in page)
            {
                List<NodeAttributeDb> nodeAttrs = attrs.Where(a => a.NodeId == nodeDb.NodeId).ToList();
                Node node = nodeDb.ToDomain(nodeAttrs);
                resultNodes.Add(node);
            }

            CursorResult<Node> result = new CursorResult<Node>(
                resultNodes,
                cursor,
                nextCursor,
                hasMore,
                mode: null,
                next: null);

            return result;
        }

        private static IReadOnlyList<NodeAttributeKey> ResolveQueryKeys(SearchNodesFilter filter, INodeKindHandler handler)
        {
            if (filter.QueryAttributeKeys != null && filter.QueryAttributeKeys.Count > 0)
            {
                List<NodeAttributeKey> keys = new List<NodeAttributeKey>();
                foreach (string key in filter.QueryAttributeKeys)
                {
                    keys.Add(new NodeAttributeKey(key));
                }
                return keys;
            }

            return handler.GetSearchAttributeKeys();
        }

        private static IReadOnlyList<NodeAttributeKey> ResolveProjectionKeys(SearchNodesFilter filter, INodeKindHandler handler)
        {
            if (filter.AttributeKeys != null && filter.AttributeKeys.Count > 0)
            {
                List<NodeAttributeKey> keys = new List<NodeAttributeKey>();
                foreach (string key in filter.AttributeKeys)
                {
                    keys.Add(new NodeAttributeKey(key));
                }
                return keys;
            }

            return handler.GetSummaryAttributeKeys();
        }

        private async Task<List<NodeAttributeDb>> LoadProjectedAttributesAsync(List<Guid> nodeIds, IReadOnlyList<NodeAttributeKey> projectionKeys, string? locale, CancellationToken cancellationToken)
        {
            List<string> keyStrings = projectionKeys.Select(x => x.Key).Distinct().ToList();

            IQueryable<NodeAttributeDb> query = _context.NodeAttributes.AsNoTracking()
                .Where(a => nodeIds.Contains(a.NodeId))
                .Where(a => keyStrings.Contains(a.Key));

            if (!string.IsNullOrWhiteSpace(locale))
            {
                string requested = locale!;
                query = query.Where(a => a.Locale == requested || a.Locale == null || a.Locale == "en");
            }

            List<NodeAttributeDb> candidates = await query.ToListAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(locale))
            {
                return candidates;
            }

            return PickBestLocaleAttributes(candidates, locale!);
        }

        private static List<NodeAttributeDb> PickBestLocaleAttributes(List<NodeAttributeDb> candidates, string requestedLocale)
        {
            Dictionary<(Guid NodeId, string Key), NodeAttributeDb> best = new Dictionary<(Guid, string), NodeAttributeDb>();

            foreach (NodeAttributeDb attribute in candidates)
            {
                (Guid NodeId, string Key) groupKey = (attribute.NodeId, attribute.Key);

                if (!best.TryGetValue(groupKey, out NodeAttributeDb? current))
                {
                    best[groupKey] = attribute;
                    continue;
                }

                int currentScore = GetLocaleScore(current.Locale, requestedLocale);
                int newScore = GetLocaleScore(attribute.Locale, requestedLocale);

                if (newScore > currentScore)
                {
                    best[groupKey] = attribute;
                }
            }

            return best.Values.ToList();
        }

        private static int GetLocaleScore(string? locale, string requestedLocale)
        {
            if (locale == requestedLocale)
            {
                return 3;
            }

            if (locale == null)
            {
                return 2;
            }

            if (locale == "en")
            {
                return 1;
            }

            return 0;
        }

        private async Task<Node> LoadNodeAsync(Guid nodeId, CancellationToken cancellationToken)
        {
            NodeDb nodeDb = await _context.Nodes.AsNoTracking()
                .SingleAsync(x => x.NodeId == nodeId, cancellationToken);

            List<NodeAttributeDb> attrs = await _context.NodeAttributes.AsNoTracking()
                .Where(x => x.NodeId == nodeId)
                .ToListAsync(cancellationToken);

            return nodeDb.ToDomain(attrs);
        }

        private static void ApplyPatch(Guid nodeId, List<NodeAttributeDb> existing, PatchNodeAttributesCommand patch)
        {
            IEnumerable<NodeAttributeKey> removes = patch.Remove ?? Array.Empty<NodeAttributeKey>();
            foreach (NodeAttributeKey remove in removes)
            {
                NodeAttributeDb? found = existing.FirstOrDefault(x => x.Key == remove.Key && x.Locale == remove.Locale);

                if (found != null)
                {
                    existing.Remove(found);
                }
            }

            IEnumerable<NodeAttribute> sets = patch.Set ?? Array.Empty<NodeAttribute>();
            foreach (NodeAttribute set in sets)
            {
                NodeAttributeDb? found = existing.FirstOrDefault(x => x.Key == set.Key && x.Locale == set.Locale);

                if (found == null)
                {
                    NodeAttributeDb created = set.ToDb(nodeId);
                    existing.Add(created);
                }
                else
                {
                    found.ApplyFromDomain(set);
                }
            }
        }
    }
}
