using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class Day
    {
        public string DayOfWeek { get; set; }
        public DateTime? Date { get; set; }
        public bool IsOpen { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
