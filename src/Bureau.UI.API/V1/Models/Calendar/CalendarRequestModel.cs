using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.API.V1.Models.Calendar
{
    public class CalendarRequestModel
    {
        [Required]
        public required string Name { get; set; }

        public string? Description { get; set; }
    }
}
