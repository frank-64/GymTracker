using GymTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Interfaces
{
    public interface ITrackingService
    {
        public Task IncrementCountAsync(int amount);
        public Task DecrementCountAsync(int amount);
        public Task<GymStatus> GetGymStatusAsync();
        public Task<GymInsightsDTO> GetGymInsightsAsync();
        public Task UpdateOverallGymInsightsAsync();
        public Task UpdateHourlyGymInsightsAsync();
        public Task UpdateGymStatusAsync(GymStatus gymStatus);
        public Task<GymDayTracker> GetGymDayTrackerAsync();
    }
}
