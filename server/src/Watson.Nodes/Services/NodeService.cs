using Bureau;
using Bureau.Primitives.Errors;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using Watson.Nodes.Abstractions.Conventions;
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

        public async Task<CursorResult<Node>> SearchAsync(SearchNodesQuery query, CancellationToken cancellationToken = default)
        {
            SearchNodesFilter filter = query.Filter;

            INodeKindHandler handler = GetHandler(filter.Kind);

            Result validation = handler.ValidateSearch(query);
            if (validation.IsError)
            {
                return validation.Error;
            }

            IQueryable<NodeDb> baseQuery = BuildBaseSearchQuery(filter, query.Cursor.Cursor);

            if (!string.IsNullOrWhiteSpace(filter.Query))
            {
                baseQuery = ApplyFilterOnSearchQuery(baseQuery, filter, handler);
            }

            int limit = query.Cursor.Limit;
            // Limit+1 pattern for HasMore
            List<NodeDb> page = await baseQuery.OrderBy(x => x.CreatedSequence).Take(limit + 1).ToListAsync(cancellationToken);

            bool hasMore = page.Count > limit;
            if (hasMore)
            {
                page.RemoveAt(page.Count - 1);
            }
            long cursor = query.Cursor.Cursor;
            if (page.Count > 0)
            {
                cursor = page[page.Count - 1].CreatedSequence;
            }

            List<Guid> nodeIds = [.. page.Select(x => x.NodeId)];
            List<string> projectionKeys = handler.GetProjectionAttributeKeys(filter.AttributeKeys);
            Dictionary<Guid, List<NodeAttributeDb>> attrs = await LoadProjectedAttributesAsync(nodeIds, projectionKeys, filter.Locale, cancellationToken);

            List<Node> resultNodes = new List<Node>();
            foreach (NodeDb nodeDb in page)
            {
                attrs.TryGetValue(nodeDb.NodeId, out List<NodeAttributeDb>? nodeAttrs);
                Node node = nodeDb.ToDomain(nodeAttrs);
                resultNodes.Add(node);
            }

            return new CursorResult<Node>(resultNodes, query.Cursor, cursor, hasMore);
        }

        private IQueryable<NodeDb> ApplyFilterOnSearchQuery(IQueryable<NodeDb> baseQuery, SearchNodesFilter filter, INodeKindHandler handler)
        {
            List<string> searchKeys = handler.GetSearchAttributeKeys(filter.QueryAttributeKeys);

            ParsedQuery parsed = new ParsedQuery(filter.Query);

            IQueryable<NodeAttributeDb> attrQuery = _context.NodeAttributes.AsNoTracking()
                    .Where(a => searchKeys.Contains(a.Key));

            if (!string.IsNullOrWhiteSpace(filter.Locale))
            {
                attrQuery = attrQuery.Where(a => a.Locale == filter.Locale || a.Locale == LocaleConventions.DefaultLocale || a.Locale == LocaleConventions.FallbackLocale);
            }

            Expression<Func<NodeAttributeDb, bool>> match = BuildMatchPredicate(parsed);

            return baseQuery.Where(n => attrQuery.Where(a => a.NodeId == n.NodeId).Any(match));
        }

        private static Expression<Func<NodeAttributeDb, bool>> BuildMatchPredicate(ParsedQuery parsed)
        {
            return a =>
                (parsed.HasText && a.Type == AttributeValueType.String && a.ValueString != null && EF.Functions.Like(a.ValueString, "%" + parsed.Text + "%")) ||
                (parsed.HasText && a.Type == AttributeValueType.Json && a.ValueJson != null && EF.Functions.Like(a.ValueJson, "%" + parsed.Text + "%")) ||
                (parsed.HasNumber && a.Type == AttributeValueType.Number && a.ValueNumber != null && a.ValueNumber == parsed.Number) ||
                (parsed.HasBool && a.Type == AttributeValueType.Bool && a.ValueBool != null && a.ValueBool == parsed.Bool) ||
                (parsed.HasGuid && a.Type == AttributeValueType.Ref && a.RefNodeId != null && a.RefNodeId == parsed.Guid) ||
                (parsed.HasDate && a.Type == AttributeValueType.Date && a.ValueDate != null && a.ValueDate == parsed.Date);
        }

        private IQueryable<NodeDb> BuildBaseSearchQuery(SearchNodesFilter filter, long cursor)
        {
            IQueryable<NodeDb> query = _context.Nodes.AsNoTracking()
                .Where(x => x.Kind == filter.Kind)
                .Where(x => filter.Scope == null || x.Scope == filter.Scope)
                .Where(x => x.CreatedSequence > cursor);

            return query;
        }

        private async Task<Dictionary<Guid, List<NodeAttributeDb>>> LoadProjectedAttributesAsync(List<Guid> nodeIds, List<string> projectionKeys, string? locale, CancellationToken cancellationToken)
        {
            IQueryable<NodeAttributeDb> query = _context.NodeAttributes.AsNoTracking()
                .Where(a => nodeIds.Contains(a.NodeId))
                .Where(a => projectionKeys.Contains(a.Key));

            if (!string.IsNullOrWhiteSpace(locale))
            {
                query = query.Where(a => a.Locale == locale || a.Locale == LocaleConventions.DefaultLocale || a.Locale == LocaleConventions.FallbackLocale);
            }

            List<NodeAttributeDb> candidates = await query.ToListAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(locale))
            {
                candidates = PickBestLocaleAttributes(candidates, locale);
            }

            return candidates.GroupBy(x => x.NodeId).ToDictionary(g => g.Key, g => g.ToList());
        }

        private static List<NodeAttributeDb> PickBestLocaleAttributes(List<NodeAttributeDb> candidates, string requestedLocale)
        {
            Dictionary<(Guid NodeId, string Key), NodeAttributeDb> best = new Dictionary<(Guid, string), NodeAttributeDb>();

            foreach (NodeAttributeDb attribute in candidates)
            {
                (Guid NodeId, string Key) groupKey = (attribute.NodeId, attribute.Key);

                if (!best.TryGetValue(groupKey, out NodeAttributeDb? currentBest))
                {
                    best[groupKey] = attribute;
                    continue;
                }

                int currentBestScore = LocaleConventions.GetLocaleScore(currentBest.Locale, requestedLocale);
                int newScore = LocaleConventions.GetLocaleScore(attribute.Locale, requestedLocale);

                if (newScore > currentBestScore)
                {
                    best[groupKey] = attribute;
                }
            }

            return [.. best.Values];
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
            if (patch.Remove is not null)
            {
                foreach (NodeAttributeKey remove in patch.Remove)
                {
                    NodeAttributeDb? found = existing.FirstOrDefault(x => x.Key == remove.Key && x.Locale == remove.Locale);

                    if (found != null)
                    {
                        existing.Remove(found);
                    }
                }
            }

            if (patch.Set is not null)
            {
                foreach (NodeAttribute set in patch.Set)
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

        private sealed class ParsedQuery
        {
            public string Text { get; }
            public bool HasText { get; }
            public bool HasNumber { get; }
            public decimal Number { get; }
            public bool HasBool { get; }
            public bool Bool { get; }
            public bool HasGuid { get; }
            public Guid Guid { get; }
            public bool HasDate { get; }
            public DateTimeOffset Date { get; }

            public ParsedQuery(string? text)
            {
                Text = text!.Trim();
                HasText = !string.IsNullOrWhiteSpace(Text);

                HasNumber = decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal n);
                Number = n;

                HasBool = TryParseBool(text, out bool b);
                Bool = b;

                HasGuid = Guid.TryParse(text, out Guid g);
                Guid = g;

                HasDate = DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset d);
                Date = d;
            }

            private static bool TryParseBool(string input, out bool value)
            {
                string normalized = input.Trim().ToLowerInvariant();

                if (normalized == "true" || normalized == "1" || normalized == "yes")
                {
                    value = true;
                    return true;
                }

                if (normalized == "false" || normalized == "0" || normalized == "no")
                {
                    value = false;
                    return true;
                }

                value = default;
                return false;
            }
        }
    }
}
