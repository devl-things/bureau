using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    public interface ITag
    {
        public string Id { get; }
        public string Label { get; }
    }
}
