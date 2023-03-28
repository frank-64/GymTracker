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
        private readonly string keyVaultName;
        private readonly IAzureRepository _azureRepository;

        public GymDetailsService(IAzureRepository azureRepository)
        {
            _azureRepository = azureRepository;
            blobName = Environment.GetEnvironmentVariable("gymDetailsBlobName");
            keyVaultName = Environment.GetEnvironmentVariable("keyVaultName");
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
            var kvUri = $"https://{keyVaultName}.vault.azure.net";

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            string secret = "";

            var secretResponse = await client.GetSecretAsync(credentials.Username);
            secret = secretResponse.Value.Value;

            // Check if password provided matches stored hash password 
            var valid = BCrypt.Net.BCrypt.Verify(credentials.Password, secret);
            return valid;
        }
    }
}
