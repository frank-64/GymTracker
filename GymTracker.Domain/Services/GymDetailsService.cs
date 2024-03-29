﻿using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Globalization;

namespace GymTracker.Domain.Services
{
    public class GymDetailsService : IGymDetailsService
    {
        private readonly string blobName;
        private readonly string adminDatabaseId;
        private readonly string adminContainerId;
        private readonly string trackingDatabaseId;
        private readonly string trackingContainerId;
        private readonly IBlobRepository _blobRepository;
        private readonly ICosmosRepository _cosmosRepository;

        public GymDetailsService(IBlobRepository azureRepository, ICosmosRepository cosmosRepository)
        {
            _blobRepository = azureRepository;
            _cosmosRepository = cosmosRepository;
            blobName = Environment.GetEnvironmentVariable("gymDetailsBlobName");
            adminDatabaseId = Environment.GetEnvironmentVariable("adminDatabaseId");
            adminContainerId = Environment.GetEnvironmentVariable("adminContainerId");
            trackingDatabaseId = Environment.GetEnvironmentVariable("trackingDatabaseId");
            trackingContainerId = Environment.GetEnvironmentVariable("trackingContainerId");
        }

        public async Task<GymDetails> GetGymDetails()
        {
            return await _blobRepository.GetBlob<GymDetails>(blobName);
}

        public async Task<bool> DetermineGymStatus(Day customOpeningHours)
        {
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            if (customOpeningHours == null) // If not custom opening hours have been set use the preset gym opening hours from gymDetails
            {
                // Getting the gym details from Blob Storage
                GymDetails gymDetails = await GetGymDetails();

                // Filtering the day in the opening hours to find the opening hours for the current day of the week
                Day day = gymDetails.OpeningHours.Where(day => day.DayOfWeek.ToLower() == DateTime.Now.DayOfWeek.ToString().ToLower()).Single();

                startTime = DateTime.Parse(day.StartTime);
                endTime = DateTime.Parse(day.EndTime);
            }
            else // An admin has set custom opening hours for today so use those values instead to determine if the gym is open as of execution
            {
                startTime = DateTime.Parse(customOpeningHours.StartTime);
                endTime = DateTime.Parse(customOpeningHours.EndTime);
            }

            // Return true or false depending if current time falls in the start/end opening period for the current day
            return DateTime.Now.TimeOfDay >= startTime.TimeOfDay && DateTime.Now.TimeOfDay <= endTime.TimeOfDay;
        }

        public async Task<int> GetMaximumOccupancy()
        {
            // Getting the gym details from Blob Storage
            GymDetails gymDetails = await GetGymDetails();

            return gymDetails.MaxOccupancy;
        }

        public async Task UpdateGymDetails(GymDetails updatedGymDetails)
        {
            var currentGymDetails = await GetGymDetails();
            currentGymDetails.OpeningHours = updatedGymDetails.OpeningHours;
            await _blobRepository.UploadBlobAsync(currentGymDetails, blobName);
        }

        public async Task<bool> AdminLoginAsync(Credentials credentials)
        {
            await _cosmosRepository.CreateDatabaseAsync(adminDatabaseId);
            await _cosmosRepository.CreateContainerAsync(adminContainerId, "/id");
            
            // Get the hashed password secret from blob storage
            Secret secret = await _cosmosRepository.GetItemAsync<Secret>(credentials.Username, credentials.Username);

            // Determine if the password entered by the user matches the hashed password
            var validPassword = BCrypt.Net.BCrypt.Verify(credentials.Password, secret.HashedPassword);
            return validPassword;
        }

        public async Task SetCustomOpeningPeriod(Day customOpeningHour)
        {
            await _cosmosRepository.CreateDatabaseAsync(trackingDatabaseId);
            await _cosmosRepository.CreateContainerAsync(trackingContainerId, "/month");

            // Get GymDayTracker if it exists
            string currentMonth = customOpeningHour.Date.Value.Month.ToString();
            string stringDate = DateOnly.FromDateTime(customOpeningHour.Date.Value).ToString("dd-MM-yyyy");

            GymDayTracker gymDayTracker = new GymDayTracker();
            Day? openingHours = null;

            if (customOpeningHour.IsOpen)
            {
                openingHours = new Day()
                {
                    Date = customOpeningHour.Date.Value,
                    DayOfWeek = customOpeningHour.Date.Value.DayOfWeek.ToString(),
                    StartTime = customOpeningHour.StartTime != null ? DateTime.ParseExact(customOpeningHour.StartTime, "HH:mm", CultureInfo.InvariantCulture).ToString("h:mm tt") : null,
                    EndTime = customOpeningHour.EndTime != null ? DateTime.ParseExact(customOpeningHour.EndTime, "HH:mm", CultureInfo.InvariantCulture).ToString("h:mm tt") : null,
                    IsOpen = true
                };
            }

            ItemResponse<GymDayTracker>? gymDayTrackerItemResponse;
            try
            {
                // GymDayTracker file already exists meaning custom opening hours have previously been set for this date
                // Admin is making edits to previously added custom opening hours
                gymDayTrackerItemResponse = await _cosmosRepository.GetItemAsync<GymDayTracker>(stringDate, currentMonth); 
            }
            catch
            {
                gymDayTrackerItemResponse = null;
            }

            if (gymDayTrackerItemResponse == null) 
            {
                // GymDayTracker file has not been created yet, make a new one and upload to database with the new opening conditions
                gymDayTracker = new GymDayTracker()
                {
                    Id = stringDate,
                    Month = currentMonth,
                    CurrentDate = customOpeningHour.Date.Value,
                    IsOpen = customOpeningHour.IsOpen,
                    AdminClosedGym = !customOpeningHour.IsOpen,
                    CustomOpeningHours = customOpeningHour.IsOpen ? openingHours : null
                };
                await _cosmosRepository.AddItemToContainerAsync(gymDayTracker, gymDayTracker.Month);
            }
            else
            {
                gymDayTracker = gymDayTrackerItemResponse.Resource;
                gymDayTracker.UpdateOpeningHours(openingHours);
                await _cosmosRepository.UpsertItemAsync(gymDayTracker);
            }
        }
    }
}
