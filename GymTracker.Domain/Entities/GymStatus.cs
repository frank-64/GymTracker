using System;
namespace GymTracker.Domain.Entities
{
    public class GymStatus
    {
        public int OccupancyValue { get; set; }
        public int MaxOccupancy { get; set; }
        public double CapacityPercentage => OccupancyValue > 0 ? OccupancyValue * 100 / MaxOccupancy : 0;
        public bool IsOpen { get; set; }
        public bool AdminClosedGym { get; set; }
        public Day? CustomOpeningHours { get; set; }

        public GymStatus()
        {

        }

        public GymStatus(int occupancyValue, int maxOccupancy, bool isOpen, bool adminClosedGym, Day customOpeningHours)
        {
            OccupancyValue = occupancyValue;
            MaxOccupancy = maxOccupancy;
            IsOpen = isOpen;
            AdminClosedGym = adminClosedGym;
            CustomOpeningHours = customOpeningHours != null ? customOpeningHours : null;
        }
    }
}
