﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class PeakOccupancyDTO
    {
        public string Name { get; set; }
        public double Occupancy { get; set; }

        public PeakOccupancyDTO(string name, double occupancy)
        {
            Name = name;
            Occupancy = occupancy;
        }
    }
}
