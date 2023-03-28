using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using BCrypt.Net;

namespace GymTracker.Domain.Services
{
    public class GymDetailsService : IGymDetailsService
    {
        private readonly string blobName;
        private readonly string adminDatabaseId = "admin";
        private readonly string adminContainerId = "adminLogin";
        private readonly IAzureRepository _azureRepository;
        private readonly ICosmosRepository _cosmosRepository;

        public GymDetailsService(IAzureRepository azureRepository, ICosmosRepository cosmosRepository)
        {
            _azureRepository = azureRepository;
            _cosmosRepository = cosmosRepository;
            blobName = Environment.GetEnvironmentVariable("gymDetailsBlobName");
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
