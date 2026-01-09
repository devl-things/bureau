using Bureau;
using Watson.Nodes.Api.Mappers;
using Watson.Nodes.Contracts.Dtos;

namespace Watson.Nodes.Api.Factories
{
    public static class ChangeFeedQueryFactory
    {
        public static ChangeFeedQuery Create(ChangesQueryDto dto)
        {
            CursorParameters cursorParameters = new() { Cursor = dto.Cursor };
            cursorParameters.SetLimit(dto.Limit);
            return new ChangeFeedQuery()
            {
                Cursor = cursorParameters,
                Mode = dto.Mode.ToDomain()
            };
        }
    }
}
