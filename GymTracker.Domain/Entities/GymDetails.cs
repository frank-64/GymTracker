using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class GymDetails
    {
        public string GymName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool? AdminClosedGym { get; set; }

        // Looping through each opening hours item
        public bool IsOpen => AdminClosedGym.HasValue && AdminClosedGym.Value == true ? false : Hours.Any(hour =>
        {
            // If the day in the current item's day doesn't match the current day of week return false
            if (hour.DayOfWeek.ToLower() != DateTime.Now.DayOfWeek.ToString().ToLower())
            {
                return false;
            }

            var startTime = DateTime.Parse(hour.StartTime);
            var endTime = DateTime.Parse(hour.EndTime);

            // If the day matches and the time falls within start/end opening period and return true
            return DateTime.Now.TimeOfDay >= startTime.TimeOfDay && DateTime.Now.TimeOfDay <= endTime.TimeOfDay;
        });
        public List<Hours> Hours { get; set; }

        public GymDetails()
        {

        }
    }

    public class Hours
    {
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
