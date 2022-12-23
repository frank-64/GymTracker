using Azure;
using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly IAzureRepository _azureRepository;
        private readonly ICosmosRepository _cosmosRepository;
        private readonly string trackingFilename = "gymCounter.json";
        public TrackingService(IAzureRepository azureRepository, ICosmosRepository cosmosRepository)
        {
            _azureRepository = azureRepository;
            _cosmosRepository = cosmosRepository;
        }
        public async Task<int> GetCurrentOccupancy()
        {
            throw new NotImplementedException();
        }
        
        public async Task<int> GetTotalCapacity()
        {
            throw new NotImplementedException();
        }

        public async void IncrementCountAsync(ItemResponse<GymDayTracker> itemResponse, int amount)
        {
            GymDayTracker gymDayTracker = itemResponse.Resource;

            gymDayTracker.CurrentGymOccupancy += amount;
            if (gymDayTracker.CurrentGymOccupancy > gymDayTracker.HighestGymOccupancy)
            {
                gymDayTracker.HighestGymOccupancy = gymDayTracker.CurrentGymOccupancy;
            }

            await _cosmosRepository.UpsertItemAsync(gymDayTracker);
        }

        public async void DecrementCountAsync(ItemResponse<GymDayTracker> itemResponse, int amount)
        {
            GymDayTracker gymDayTracker = itemResponse.Resource;

            gymDayTracker.CurrentGymOccupancy -= amount;

            await _cosmosRepository.UpsertItemAsync(gymDayTracker);
        }

        public async void ManageInflux(int amount)
        {
            DateTimeOffset currentDateTimeOffset = DateTimeOffset.UtcNow;
            bool exists = await _cosmosRepository.DoesItemExistAsync(currentDateTimeOffset.Month.ToString(), currentDateTimeOffset.ToString());
            if (!exists)
            {
                await _cosmosRepository.AddItemsToContainerAsync(new GymDayTracker
                {
                    Id = currentDateTimeOffset.ToString(),
                    PartitionKey = currentDateTimeOffset.Month.ToString(),
                    CurrentDate = currentDateTimeOffset,
                    CurrentGymOccupancy = amount,
                    HighestGymOccupancy = amount,
                });
            }
            else
            {
                IncrementCountAsync(await _cosmosRepository.GetItemAsync(currentDateTimeOffset.Month.ToString(), currentDateTimeOffset.ToString()), amount);
            }
        }

        public async void ManageOutflow(int amount)
        {
            DateTimeOffset currentDateTimeOffset = DateTimeOffset.UtcNow;
            bool exists = await _cosmosRepository.DoesItemExistAsync(currentDateTimeOffset.Month.ToString(), currentDateTimeOffset.ToString());
            if (!exists)
            {
                await _cosmosRepository.AddItemsToContainerAsync(new GymDayTracker
                {
                    Id = currentDateTimeOffset.ToString(),
                    PartitionKey = currentDateTimeOffset.Month.ToString(),
                    CurrentDate = currentDateTimeOffset,
                    CurrentGymOccupancy = amount,
                    HighestGymOccupancy = amount,
                });
            }
            else
            {
                DecrementCountAsync(await _cosmosRepository.GetItemAsync(currentDateTimeOffset.Month.ToString(), currentDateTimeOffset.ToString()), amount);
            }
        }
    }
}
