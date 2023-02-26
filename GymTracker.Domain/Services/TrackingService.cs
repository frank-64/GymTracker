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
            await _cosmosRepository.CreateDatabaseAsync();
            await _cosmosRepository.CreateContainerAsync();
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            string currentMonth = currentDate.Month.ToString();
            string stringDate = currentDate.ToString("dd-MM-yyyy");
            var gymDayTracker = await _cosmosRepository.GetItemIfExistAsync(currentMonth, stringDate);
            if (gymDayTracker == null)
            {
                Console.WriteLine("Do something as missing tracker file");
                return 0;
            }
            else
            {
                return gymDayTracker.Resource.CurrentGymOccupancy;
            }
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
            gymDayTracker.LastModified = DateTimeOffset.UtcNow;

            await _cosmosRepository.UpsertItemAsync(gymDayTracker);
        }

        public async void DecrementCountAsync(ItemResponse<GymDayTracker> itemResponse, int amount)
        {
            GymDayTracker gymDayTracker = itemResponse.Resource;

            if (gymDayTracker.CurrentGymOccupancy - amount >= 0)
            {
                gymDayTracker.CurrentGymOccupancy -= amount;
            }
            else
            {
                gymDayTracker.CurrentGymOccupancy = 0;
            }
            gymDayTracker.LastModified = DateTimeOffset.UtcNow;

            await _cosmosRepository.UpsertItemAsync(gymDayTracker);
        }

        public async Task ManageInflux(int amount)
        {
            await _cosmosRepository.CreateDatabaseAsync();
            await _cosmosRepository.CreateContainerAsync();
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            DateTimeOffset currentDate = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, utcNow.Offset);
            string currentMonth = utcNow.Month.ToString();
            string stringDate = utcNow.ToString("dd-MM-yyyy");
            var gymDayTracker = await _cosmosRepository.GetItemIfExistAsync(currentMonth, stringDate);
            if (gymDayTracker == null)
            {
                await _cosmosRepository.AddItemsToContainerAsync(new GymDayTracker
                {
                    Id = stringDate,
                    PartitionKey = currentMonth,
                    CurrentDate = currentDate,
                    LastModified = utcNow,
                    CreatedOn = utcNow,
                    CurrentGymOccupancy = amount,
                    HighestGymOccupancy = amount,
                });
            }
            else
            {
                IncrementCountAsync(gymDayTracker, amount);
            }
        }

        public async Task ManageOutflow(int amount)
        {
            await _cosmosRepository.CreateDatabaseAsync();
            await _cosmosRepository.CreateContainerAsync();
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            DateTimeOffset currentDate = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, utcNow.Offset);
            string currentMonth = utcNow.Month.ToString();
            string stringDate = utcNow.ToString("dd-MM-yyyy");
            var gymDayTracker = await _cosmosRepository.GetItemIfExistAsync(currentMonth, stringDate);
            if (gymDayTracker == null)
            {
                await _cosmosRepository.AddItemsToContainerAsync(new GymDayTracker
                {
                    Id = stringDate,
                    PartitionKey = currentMonth,
                    CurrentDate = currentDate,
                    LastModified = utcNow,
                    CreatedOn = utcNow,
                    CurrentGymOccupancy = amount,
                    HighestGymOccupancy = amount,
                });
            }
            else
            {
                DecrementCountAsync(gymDayTracker, amount);
            }
        }
    }
}
