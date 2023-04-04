using Azure.Storage.Blobs;
using GymTracker.Domain;
using Newtonsoft.Json;
using System.Text;

namespace GymTracker.Repository
{
    public class BlobRepository : IBlobRepository
    {
        private readonly string connectionString;
        private readonly string containerName;
        public BlobRepository()
        {
            connectionString = Environment.GetEnvironmentVariable("storageAccountConnStr");
            containerName = Environment.GetEnvironmentVariable("blobContainerName");
        }

        public async Task UploadBlobAsync<T>(T objectToUpload, string blobName)
        {
            string json = JsonConvert.SerializeObject(objectToUpload);
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);

            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        public async Task<bool> CheckIfBlobExists(string blobPath)
        {
            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);
            return await blobClient.ExistsAsync();
        }

        public async Task<T> GetBlob<T>(string blobName)
        {
            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using Stream stream = await blobClient.OpenReadAsync();
            using StreamReader reader = new StreamReader(stream);
            return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
        }
    }
}
