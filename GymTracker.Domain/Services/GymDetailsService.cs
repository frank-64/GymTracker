using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using System.Text;
using Newtonsoft.Json;

namespace GymTracker.Domain.Services
{
    public class GymDetailsService : IGymDetailsService
    {
        private readonly string blobName;
        private readonly string adminDatabaseId;
        private readonly string adminContainerId;
        private readonly IAzureRepository _azureRepository;
        private readonly ICosmosRepository _cosmosRepository;

        public GymDetailsService(IAzureRepository azureRepository, ICosmosRepository cosmosRepository)
        {
            _azureRepository = azureRepository;
            _cosmosRepository = cosmosRepository;
            blobName = Environment.GetEnvironmentVariable("gymDetailsBlobName");
            adminDatabaseId = Environment.GetEnvironmentVariable("adminDatabaseId");
            adminContainerId = Environment.GetEnvironmentVariable("adminContainerId");
        }

        public async Task<GymDetails> GetGymDetails()
        {
            using (Stream stream = await _azureRepository.GetBlob(blobName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<GymDetails>(await reader.ReadToEndAsync());
                }
            }
        }

        public async Task<bool> DetermineGymStatus()
        {
            // Getting the gym details from Blob Storage
            GymDetails gymDetails = await GetGymDetails();

            // Filtering the day in the opening hours to find the opening hours for the current day of the week
            Day day = gymDetails.OpeningHours.Where(day => day.DayOfWeek.ToLower() == DateTime.Now.DayOfWeek.ToString().ToLower()).Single();

            var startTime = DateTime.Parse(day.StartTime);
            var endTime = DateTime.Parse(day.EndTime);

            // Return true indiciating the gym is open if the current time falls in the start/end opening period set for the current day
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
            string json = JsonConvert.SerializeObject(updatedGymDetails);
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);

            await _azureRepository.UploadBlobAsync(stream, blobName);
        }

        public async Task<bool> AdminLogin(Credentials credentials)
        {
            await _cosmosRepository.CreateDatabaseAsync(adminDatabaseId);
            await _cosmosRepository.CreateContainerAsync(adminContainerId, "/id");
            Secret secret = await _cosmosRepository.GetItemAsync<Secret>(credentials.Username, credentials.Username);
            var validPassword = BCrypt.Net.BCrypt.Verify(credentials.Password, secret.HashedPassword);
            return validPassword;
        }
    }
}
