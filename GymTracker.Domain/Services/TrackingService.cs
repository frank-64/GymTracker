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
        private readonly ICosmosRepository _cosmosRepository;
        private readonly int _maximumOccupancy;
        // The name of the database and container we will create
        private string trackingDatabaseId = "tracking";
        private string trackingContainerId = "dailyTracked";
        public TrackingService(ICosmosRepository cosmosRepository)
        {
            _cosmosRepository = cosmosRepository;
            _maximumOccupancy = int.Parse(Environment.GetEnvironmentVariable("maxGymOccupancy"));
        }
        public async Task<Occupancy> GetCurrentOccupancy()
        {
            await _cosmosRepository.CreateDatabaseAsync(trackingDatabaseId);
            await _cosmosRepository.CreateContainerAsync(trackingContainerId, "/partitionKey");
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            string currentMonth = currentDate.Month.ToString();
            string stringDate = currentDate.ToString("dd-MM-yyyy");
            var gymDayTracker = await _cosmosRepository.GetItemIfExistAsync<GymDayTracker>(currentMonth, stringDate);
            if (gymDayTracker == null)
            {
                Console.WriteLine("Do something as missing tracker file");
                return new Occupancy();
            }
            else
            {
                Occupancy occupancy = new Occupancy(gymDayTracker.Resource.CurrentGymOccupancy, _maximumOccupancy);
                return occupancy;
            }
        }

        public async void IncrementCountAsync(ItemResponse<GymDayTracker> itemResponse, int amount)
        {
            GymDayTracker gymDayTracker = itemResponse.Resource;

            gymDayTracker.CurrentGymOccupancy += amount;

            if (!(gymDayTracker.CurrentGymOccupancy >= _maximumOccupancy))
            {
                // Set highest occupancy if more than current highest occupancy
                if (gymDayTracker.CurrentGymOccupancy > gymDayTracker.HighestGymOccupancy)
                {
                    gymDayTracker.HighestGymOccupancy = gymDayTracker.CurrentGymOccupancy;
                }

                // Update last modified time of item
                gymDayTracker.LastModified = DateTimeOffset.UtcNow;

                //Update item
                await _cosmosRepository.UpsertItemAsync(gymDayTracker);
            }
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
            await _cosmosRepository.CreateDatabaseAsync(trackingDatabaseId);
            await _cosmosRepository.CreateContainerAsync(trackingContainerId, "/partitionKey");
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            DateTimeOffset currentDate = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, utcNow.Offset);
            string currentMonth = utcNow.Month.ToString();
            string stringDate = utcNow.ToString("dd-MM-yyyy");
            var gymDayTracker = await _cosmosRepository.GetItemIfExistAsync<GymDayTracker>(currentMonth, stringDate);
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
            await _cosmosRepository.CreateDatabaseAsync(trackingDatabaseId);
            await _cosmosRepository.CreateContainerAsync(trackingContainerId, "/partitionKey");
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            DateTimeOffset currentDate = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, utcNow.Offset);
            string currentMonth = utcNow.Month.ToString();
            string stringDate = utcNow.ToString("dd-MM-yyyy");
            var gymDayTracker = await _cosmosRepository.GetItemIfExistAsync<GymDayTracker>(currentMonth, stringDate);
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
