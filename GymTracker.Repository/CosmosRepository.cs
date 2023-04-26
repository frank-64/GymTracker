using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;
using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using System.ComponentModel;
using System.Reflection.Metadata;
using Container = Microsoft.Azure.Cosmos.Container;

namespace CosmosGettingStartedTutorial
{
    public class CosmosRepository : ICosmosRepository
    {
        private readonly string EndpointUri;
        private readonly string PrimaryKey;
        private CosmosClient _cosmosClient;
        private Database _database;
        private Container _container;

        public CosmosRepository()
        {
            EndpointUri = Environment.GetEnvironmentVariable("dbEndpointUri");
            PrimaryKey = Environment.GetEnvironmentVariable("dbPrimaryKey");
            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
        }

        public async Task CreateDatabaseAsync(string databaseId)
        {
            // Create a new database
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        public async Task CreateContainerAsync(string containerId, string partitionKey)
        {
            // Create a new container
            _container = await _database.CreateContainerIfNotExistsAsync(containerId, partitionKey);
        }

        public async Task<ItemResponse<Item>> GetItemAsync<Item>(string id, string partitionKey)
        {
            // Read the item
            var response = await _container.ReadItemAsync<Item>(id, new PartitionKey(partitionKey));
            Console.WriteLine($"Reading an item from the database consumed {response.RequestCharge} RUs.");
            return response;
        }

        public async Task UpsertItemAsync<Item>(Item item)
        {
            var response = await _container.UpsertItemAsync(item);
            Console.WriteLine($"Upserted item to the database consumed {response.RequestCharge} RUs.");
        }

        public async Task AddItemToContainerAsync<T>(T item, string partitionKey)
        {
            ItemResponse<T> gymDayTrackerResponse = await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
            // Output the RUs (Request Units) used, only for testing purposes
            Console.WriteLine($"Created item in database Operation consumed {gymDayTrackerResponse.RequestCharge} RUs.");
            if (gymDayTrackerResponse.StatusCode != HttpStatusCode.Created)
            {
                throw new Exception($"Failed to create item with id: {gymDayTrackerResponse.ActivityId}. StatusCode: {gymDayTrackerResponse.StatusCode}");
            }
        }
    }
}
