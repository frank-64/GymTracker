using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace GymTracker.Domain.Services
{
	public class TrackingService : ITrackingService
	{
		private readonly ICosmosRepository _cosmosRepository;
		private readonly IGymDetailsService _gymDetailsService;
		private readonly IBlobRepository _blobRepository;

		private string trackingDatabaseId;
		private string trackingContainerId;
		private readonly string blobName;
		public TrackingService(ICosmosRepository cosmosRepository, IBlobRepository blobRepository, IGymDetailsService gymDetailsService)
		{
			_cosmosRepository = cosmosRepository;
			_gymDetailsService = gymDetailsService;
			_blobRepository = blobRepository;
			trackingDatabaseId = Environment.GetEnvironmentVariable("trackingDatabaseId");
			trackingContainerId = Environment.GetEnvironmentVariable("trackingContainerId");
			blobName = Environment.GetEnvironmentVariable("gymInsightsBlobName");
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

		public async Task<GymInsightsDTO> GetGymInsightsAsync()
		{
			var maxOccupancy = await _gymDetailsService.GetMaximumOccupancy();
			// Get the current day of the week
			string currentDayOfWeek = DateTime.Now.DayOfWeek.ToString();
			List<GymInsights> gymInsights = await _blobRepository.GetBlob<List<GymInsights>>(blobName);
			// TODO: Implement the collection of gym insights from blob
			// Put them into DTO object ready to be displayed in the frontend

			// Days
			List<PeakOccupancyDTO> dailyPeakOccupancyAverages = new List<PeakOccupancyDTO>();

			// Iterating over each day of the week
			foreach (DaysOfTheWeek day in Enum.GetValues(typeof(DaysOfTheWeek)))
			{
				string dayShorthand = day.ToString() == "Thursday" ? "Thurs" : day.ToString().Substring(0, 3);

				// Filter insights for the current day of the week
				var insightsForDay = gymInsights.Where(i => i.DayOfWeek == day.ToString());

				// Getting the average max occupancy reached for current day of the week
				double averageMaxOccupancy = insightsForDay.Average(i => i.MaxOccupancyReached);

				// Store the average in the dictionary
				dailyPeakOccupancyAverages.Add(new PeakOccupancyDTO(dayShorthand, averageMaxOccupancy, maxOccupancy));
			}

			// Hours
			Dictionary<string, int> hourlyTotals = new Dictionary<string, int>();
			Dictionary<string, int> hourlyCounts = new Dictionary<string, int>();
			List<PeakOccupancyDTO> hourlyPeakOccupancyAverages = new List<PeakOccupancyDTO>();

			// Iterate over each GymInsights object in the list
			foreach (GymInsights insights in gymInsights)
			{
				// Check if the DayOfWeek matches the current day of the week
				if (insights.DayOfWeek == currentDayOfWeek)
				{
					// Iterate over each hour in the HourlyPeakOccupancy dictionary
					foreach (KeyValuePair<string, int> entry in insights.HourlyPeakOccupancy)
					{
						string hour = entry.Key;
						int occupancy = entry.Value;

                        //// If the hour doesn't exist in the hourlyTotals dictionary, add it with a value of 0
                        //if (!hourlyTotals.ContainsKey(hour))
                        //{
                        //    hourlyTotals.Add(hour, 0);
                        //    hourlyCounts.Add(hour, 0);
                        //}

                        // Add the current occupancy to the hourly total and increment the hourly count
                        try
                        {
							hourlyTotals[hour] += occupancy;
							hourlyCounts[hour]++;
                        }
                        catch
                        {
							// Adding new hour to dictionaries
							hourlyTotals.Add(hour, occupancy);
							hourlyCounts.Add(hour, 1);
                        }
					}
				}
			}

			// Calculate the hourly averages
			foreach (KeyValuePair<string, int> entry in hourlyTotals)
			{
				string hour = entry.Key;
				int totalOccupancy = entry.Value;
				int count = hourlyCounts[hour];

				double averageOccupancy = (double)totalOccupancy / count;
				hourlyPeakOccupancyAverages.Add(new PeakOccupancyDTO(hour, averageOccupancy, maxOccupancy));
			}

			// TODO: Append current equipment availability to gym insights as that is based on current occupancy in the gym vs total equipment and depmand for each equipment
			// Some math will be required for this estimate

			return new GymInsightsDTO { DayOfWeek= currentDayOfWeek, AverageDailyPeakOccupancy = dailyPeakOccupancyAverages, AverageHourlyPeakOccupancy = hourlyPeakOccupancyAverages};
		}

		public async Task UpdateHourlyGymInsightsAsync()
		{
			GymDayTracker gymDayTracker = await GetGymDayTrackerAsync();
			if (!gymDayTracker.IsOpen) // Gym is closed so try update insights
			{
				return;
			}

			DateTime now = DateTime.Now;
			// Go back one hour to display the max occupancy recorded during the previous hour
			DateTime nowWithoutMinutes = new DateTime(now.Year, now.Month, now.Day, now.Hour-1, 0, 0);
			var stringTime = nowWithoutMinutes.ToString("h:mm tt");

			List<GymInsights> gymInsights = await _blobRepository.GetBlob<List<GymInsights>>(blobName);
			GymInsights todaysInsights = gymInsights.FirstOrDefault(i => i.Date == now.Date);

			if (todaysInsights != null)
			{
				// Existing insights object so append new hourly insight to it
				gymInsights.Where(i => i.Date == now.Date).Single().UpdateHourlyInsights(now.Date, stringTime, gymDayTracker.CurrentGymOccupancy);
			}
			else
			{
				// Gym has just opened so create today's a new insight object
				todaysInsights = new GymInsights(); 
				todaysInsights.UpdateHourlyInsights(now.Date, stringTime, gymDayTracker.CurrentGymOccupancy);
				gymInsights.Add(todaysInsights);
			}

			await _blobRepository.UploadBlobAsync(gymInsights, blobName);
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

			// Increment the current occupancy with a limit of the max occupancy value
			gymDayTracker.CurrentGymOccupancy = gymDayTracker.CurrentGymOccupancy + amount > gymDayTracker.MaximumOccupancy ? gymDayTracker.MaximumOccupancy : gymDayTracker.CurrentGymOccupancy + amount;

			await _cosmosRepository.UpsertItemAsync(gymDayTracker);
		}

		public async Task DecrementCountAsync(int amount)
		{
			GymDayTracker gymDayTracker = await GetGymDayTrackerAsync();

			// Decrement the current occupancy count with a floor limit of 0
			gymDayTracker.CurrentGymOccupancy = gymDayTracker.CurrentGymOccupancy - amount >= 0 ? gymDayTracker.CurrentGymOccupancy - amount : 0;

			await _cosmosRepository.UpsertItemAsync(gymDayTracker);
		}
	}
}
