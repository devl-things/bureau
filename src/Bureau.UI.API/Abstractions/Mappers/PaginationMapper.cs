using Bureau.Core;
using Bureau.UI.API.Models;

namespace Bureau.UI.API.Mappers
{
    internal static class PaginationMapper
    {
        public static PaginationData ToPaginationData(this PaginationMetadata pagination) 
        {
            return new PaginationData()
            {
                CurrentPage = pagination.CurrentPage,
                PageSize = pagination.PageSize,
                TotalItems = pagination.TotalItems,
                TotalPages = pagination.TotalPages,
            };
        }
    }
}
