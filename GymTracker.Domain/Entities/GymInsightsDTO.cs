using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class GymInsightsDTO
    {
        public List<PeakOccupancyDTO> AverageDailyPeakOccupancy { get; set; }
        public string DayOfWeek { get; set; } // Day of the week for the average hourly peak occupancy
        public List<PeakOccupancyDTO> AverageHourlyPeakOccupancy { get; set; }
    }
}
