using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        internal Reference(string id)
        {
            Id = id;
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is IReference reference)
            {
                return reference.Id == Id;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return base.GetHashCode();
            }
            return Id.GetHashCode();
        }
    }
}
