using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using Microsoft.Azure.Cosmos;

namespace GymTracker.Domain.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly ICosmosRepository _cosmosRepository;
        private readonly IGymDetailsService _gymDetailsService;

        private string trackingDatabaseId;
        private string trackingContainerId;
        public TrackingService(ICosmosRepository cosmosRepository, IGymDetailsService gymDetailsService)
        {
            _cosmosRepository = cosmosRepository;
            _gymDetailsService = gymDetailsService;
            trackingDatabaseId = Environment.GetEnvironmentVariable("trackingDatabaseId");
            trackingContainerId = Environment.GetEnvironmentVariable("trackingContainerId");
        }

        public async Task<GymDayTracker> GetGymDayTrackerAsync()
        {
            // Setting up database and container locally
            await _cosmosRepository.CreateDatabaseAsync(trackingDatabaseId);
            await _cosmosRepository.CreateContainerAsync(trackingContainerId, "/month");

            // Getting the current date and month as these are the id and partitionKey respectively for the GymDayTracker file
            DateTimeOffset currentDateTime = DateTime.Now;
            string currentMonth = currentDateTime.Month.ToString();
            string stringDate = DateOnly.FromDateTime(currentDateTime.Date).ToString("dd-MM-yyyy");
            DateTimeOffset currentDate = new DateTimeOffset(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 0, 0, 0, currentDateTime.Offset);

            GymDayTracker gymDayTracker = null;
            ItemResponse<GymDayTracker> gymDayTrackerItemResponse;
            try
            {
                gymDayTrackerItemResponse = await _cosmosRepository.GetItemAsync<GymDayTracker>(stringDate, currentMonth);
            }
            catch
            {
                gymDayTrackerItemResponse = null;
            }


            if (gymDayTrackerItemResponse == null) // GymDayTracker file has not been created yet, make a new one
            {
                gymDayTracker = new GymDayTracker {
                    Id = stringDate,
                    Month = currentMonth,
                    CurrentDate = currentDate,
                    IsOpen = false,
                    AdminClosedGym = false
                };
                await _cosmosRepository.AddGymDayTrackerToContainerAsync(gymDayTracker); // GymDayTracker file already exists
            }
            else
            {
                gymDayTracker = gymDayTrackerItemResponse.Resource;
            }

            // If Admin has closed the gym don't do anything else and return
            if (gymDayTracker.AdminClosedGym)
            {
                gymDayTracker.IsOpen = false;
                return gymDayTracker;
            }

            var isOpen = await _gymDetailsService.DetermineGymStatus(gymDayTracker.CustomOpeningHours);
            var maxOccupancy = await _gymDetailsService.GetMaximumOccupancy();
            gymDayTracker.IsOpen = isOpen;
            gymDayTracker.MaximumOccupancy = maxOccupancy;
            return gymDayTracker;
        }

        public async Task<GymStatus> GetGymStatusAsync()
        {
            GymDayTracker gymDayTracker = await GetGymDayTrackerAsync();

            // Return the occupancy of gym, whether it's currently open and if the admin manually closed it
            GymStatus gymStatus = new GymStatus(gymDayTracker.CurrentGymOccupancy, gymDayTracker.MaximumOccupancy, gymDayTracker.IsOpen, gymDayTracker.AdminClosedGym, gymDayTracker.CustomOpeningHours != null ? gymDayTracker.CustomOpeningHours : null);
            return gymStatus;
        }

        public async Task UpdateGymStatusAsync(GymStatus gymStatus)
        {
            GymDayTracker gymDayTracker = await GetGymDayTrackerAsync();
            gymDayTracker.AdminClosedGym = gymStatus.AdminClosedGym;

            await _cosmosRepository.UpsertItemAsync(gymDayTracker);
        }

        public async Task IncrementCountAsync(int amount)
        {
            GymDayTracker gymDayTracker = await GetGymDayTrackerAsync();

            gymDayTracker.CurrentGymOccupancy += amount;

            if (!(gymDayTracker.CurrentGymOccupancy >= gymDayTracker.MaximumOccupancy))
            {
                // Set highest occupancy if more than current highest occupancy
                if (gymDayTracker.CurrentGymOccupancy > gymDayTracker.HighestGymOccupancy)
                {
                    gymDayTracker.HighestGymOccupancy = gymDayTracker.CurrentGymOccupancy;
                }

                //Update item
                await _cosmosRepository.UpsertItemAsync(gymDayTracker);
            }
        }

        public async Task DecrementCountAsync(int amount)
        {
            GymDayTracker gymDayTracker = await GetGymDayTrackerAsync();

            if (gymDayTracker.CurrentGymOccupancy - amount >= 0)
            {
                gymDayTracker.CurrentGymOccupancy -= amount;
            }
            else
            {
                gymDayTracker.CurrentGymOccupancy = 0;
            }

            await _cosmosRepository.UpsertItemAsync(gymDayTracker);
        }
    }
}
