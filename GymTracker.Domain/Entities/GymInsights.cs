﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class GymInsights
    {
        public string DayOfWeek { get; set; }
        public DateTime Date { get; set; }
        public int MaxOccupancyReached => HourlyPeakOccupancy.Any() ? HourlyPeakOccupancy.Values.Max() : 0;
        public Dictionary<string, int> HourlyPeakOccupancy { get; set; }

        public GymInsights() { 
            HourlyPeakOccupancy = new Dictionary<string, int>();
        }

        public void UpdateHourlyInsights(DateTime date, string stringTime, int currentOccupancy)
        {
            Date = date.Date;
            DayOfWeek = date.DayOfWeek.ToString();
            HourlyPeakOccupancy.Add(stringTime, currentOccupancy);
        }
    }
}
