using Bureau.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Components.DevLMultiSelect
{
    public class Selection : ISelection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; }

        public virtual bool IsNew
        {
            get
            {
                return string.IsNullOrWhiteSpace(Id);
            }
        }
        public Selection(string name)
        {
            Name = name;
        }
        public Selection(string name, string id) : this(name)
        {
            Id = id;
        }

        public bool Contains(string name, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) 
        {
            return Name.Contains(name, comparisonType);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Selection other)
            {
                return Name == other.Name && Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Id);
        }

        public static Result<Selection> Create(string name)
        {
            return new Selection(name);
        }
    }
}
