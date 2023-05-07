using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class Equipment
    {
        public string Name { get; set; }
        public double Quantity { get; set; }
        public double RelativeDemand { get; set; }
        public double UsagePercentage { get; set; }

		public void EstimateEquipmentCapacity(double currentCapacity)
		{
			// Calculate the maximum number of users that can use the equipment
			int maxUsers = (int)(Quantity * (currentCapacity / 100));

			// Calculate the expected number of users that want to use the equipment
			double expectedUsers = maxUsers * (RelativeDemand / 100);

			// Calculate the estimated availability of this equipment
			double availability = (double)(Quantity - expectedUsers) / Quantity;

			// Set the equipment UsagePercentage which is 1 minus the percentage of the estimated availability
			UsagePercentage = Math.Round((1.0 - availability) * 100);
		}
	}
}
