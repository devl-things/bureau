using Bureau.UI.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.API.V1.Models.Calendar
{
    public class CalendarResponseModel : CalendarRequestModel, IResponseId
    {
        public required string Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
