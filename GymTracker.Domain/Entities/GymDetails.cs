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
        public List<Hours> Hours { get; set; }
        public GymDetails()
        {

        }

        public GymDetails(string gymName, string address, string phoneNumber, List<Hours> hours)
        {
            GymName = gymName;
            Address = address;
            PhoneNumber = phoneNumber;
            Hours = hours;
        }
    }

    public class Hours
    {
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
