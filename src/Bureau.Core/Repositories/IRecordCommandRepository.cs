using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Repositories
{
    public interface IRecordCommandRepository
    {
        Task<Result<IReference>> InsertAggregateAsync(InsertAggregateModel insertRequest, CancellationToken cancellationToken);
        Task<Result<IReference>> UpdateAggregateAsync(UpdateAggregateModel value, CancellationToken cancellationToken);
        Task<Result> DeleteAggregateAsync(IRemoveAggregateModel request, CancellationToken cancellationToken);
    }
}
