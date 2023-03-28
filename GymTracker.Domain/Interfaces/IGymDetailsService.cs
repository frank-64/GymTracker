using GymTracker.Domain.Entities;

namespace GymTracker.Domain.Interfaces
{
    public interface IGymDetailsService
    {
        public Task<GymDetails> GetGymDetails();
        public Task UpdateGymDetails(GymDetails updatedGymDetails);
        public Task<bool> AdminLogin(Credentials credentials);
    }
}
