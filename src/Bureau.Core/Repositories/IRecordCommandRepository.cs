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
        Task<Result<IReference>> InsertAggregateAsync(AggregateModel insertRequest, CancellationToken cancellationToken);
        Task<Result<IReference>> UpdateAggregateAsync(ExtendedAggregateModel value, CancellationToken cancellationToken);
    }
}
