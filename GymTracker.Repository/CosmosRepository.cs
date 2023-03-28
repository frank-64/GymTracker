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
            EndpointUri = Environment.GetEnvironmentVariable("endpointUri");
            PrimaryKey = Environment.GetEnvironmentVariable("primaryKey");
            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
        }

        public async Task CreateDatabaseAsync(string databaseId)
        {
            // Create a new database
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        public async Task CreateContainerAsync(string containerId, string partitionKeyFormat)
        {
            // Create a new container
            _container = await _database.CreateContainerIfNotExistsAsync(containerId, partitionKeyFormat);
        }

        public async Task<ItemResponse<Item>> GetItemAsync<Item>(string partitionKey, string id)
        {
            return await _container.ReadItemAsync<Item>(id, new PartitionKey(partitionKey));
        }

        public async Task UpsertItemAsync<Item>(Item item)
        {
            await _container.UpsertItemAsync(item);
        }

        public async Task AddItemsToContainerAsync(GymDayTracker gymDayTracker)
        {
            var item = await GetItemIfExistAsync<GymDayTracker>(gymDayTracker.PartitionKey, gymDayTracker.Id);
            
            if (item != null)
            {
                Console.WriteLine("Item in database with id: {0} already exists\n");
            }
            else
            {
                try
                {
                    ItemResponse<GymDayTracker> gymDayTrackerResponse = await _container.CreateItemAsync(gymDayTracker, new PartitionKey(gymDayTracker.PartitionKey));
                    // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                    Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", gymDayTrackerResponse.Resource.Id, gymDayTrackerResponse.RequestCharge);

                }catch(Exception ex)
                {
                    var t = ex.Message;
                }
            }
        }

        public async Task<ItemResponse<Item>> GetItemIfExistAsync<Item>(string partitionKey, string id)
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Item> response = await _container.ReadItemAsync<Item>(id, new PartitionKey(partitionKey));
                return response;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task DeleteItemAsync<Item>(string partitionKey, string id)
        {
            ItemResponse<Item> deletedResponse = await _container.DeleteItemAsync<Item>(id,new PartitionKey(partitionKey));
            Console.WriteLine("Deleted Family [{0},{1}]\n", partitionKey, id);
        }
    }
}
