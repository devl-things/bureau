using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    public interface IReference
    {
        public string Id { get; set; }
    }

    public struct Reference : IReference
    {
        public string Id { get; set; }
        public Reference(string id)
        {
            Id = id;
        }
    }
}
