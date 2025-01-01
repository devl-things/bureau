using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Factories
{
    public class ResultErrorFactory
    {
        public static ResultError InvalidRecord(string recordId) => new ResultError($"Flexible record with Id = {recordId} is invalid.");
        public static ResultError InvalidRecord(string recordId, Exception ex) => new ResultError($"Flexible record with Id = {recordId} is invalid.", ex);

    }
}
