using Azure.Storage.Blobs;
using GymTracker.Domain;

namespace GymTracker.Repository
{
    public class AzureRepository : IAzureRepository
    {
        private readonly string connectionString;
        private readonly string containerName = "gym-details";
        public AzureRepository()
        {
            connectionString = Environment.GetEnvironmentVariable("GymDetailsBlobConnStr");
        }

        public async Task UploadBlobAsync(Stream stream, string blobName)
        {
            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(stream);
        }

        public async Task<bool> CheckIfBlobExists(string blobPath)
        {
            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);
            return await blobClient.ExistsAsync();
        }

        public async Task<Stream> GetBlob(string blobName)
        {
            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            return await blobClient.OpenReadAsync();
        }
    }
}
