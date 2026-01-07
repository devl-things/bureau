using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Factories
{
    public static class SearchNodesQueryFactory
    {
        public static SearchNodesQuery Create(NodeKind kind, SearchNodesQueryDto dto)
        {
            CursorParameters cursorParameters = new() { Cursor = dto.Cursor };
            cursorParameters.SetLimit(dto.Limit);
            SearchNodesFilter filter = new()
            {
                Kind = kind,
                Query = dto.Query,
                Scope = dto.Scope,
                Locale = dto.Locale,
                AttributeKeys = dto.Attributes
            };
            return new SearchNodesQuery(filter, cursorParameters);
        }
    }
}
