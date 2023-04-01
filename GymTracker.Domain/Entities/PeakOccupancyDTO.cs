using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class PeakOccupancyDTO
    {
        public string name { get; set; }
        public double occupancy { get; set; }

        public PeakOccupancyDTO(string nameParam, double occupancyParam, int maxOccupancy)
        {
            name = nameParam;
            occupancy = occupancyParam > 0 ? occupancyParam * 100 / maxOccupancy : 0;
        }
    }
}
