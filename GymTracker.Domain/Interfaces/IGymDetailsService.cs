﻿using GymTracker.Domain.Entities;

namespace GymTracker.Domain.Interfaces
{
    public interface IGymDetailsService
    {
        public Task<GymDetails> GetGymDetails();
        public Task<bool> DetermineGymStatus(Day day);
        public Task<int> GetMaximumOccupancy();
        public Task UpdateGymDetails(GymDetails updatedGymDetails);
        public Task<bool> AdminLoginAsync(Credentials credentials);
        public Task SetCustomOpeningPeriod(Day customOpeningHour);
    }
}
