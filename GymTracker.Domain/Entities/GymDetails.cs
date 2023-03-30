﻿using System;
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
        public int MaxOccupancy { get; set; } = 100;
        public List<Day> OpeningHours { get; set; }

        public GymDetails()
        {

        }
    }

    public class Day
    {
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
