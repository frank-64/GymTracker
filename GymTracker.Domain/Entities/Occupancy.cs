using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class Occupancy
    {
        public int Value { get; set; }
        public int Percentage {get; set; }
        public int MaxOccupancy { get; set; }

        public Occupancy()
        {

        }
        public Occupancy(int value, int maxOccupancy)
        {
            this.Value = value;
            this.MaxOccupancy = maxOccupancy;
            if (value > 0)
            {
                this.Percentage = MaxOccupancy / value;
            }
        }
    }
}
