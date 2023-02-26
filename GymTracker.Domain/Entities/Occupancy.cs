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
        public int Percentage => Value / 100;
    }
}
