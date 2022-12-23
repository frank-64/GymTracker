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
        // The Azure Cosmos DB endpoint for running this sample.
        private readonly string EndpointUri;
        // The primary key for the Azure Cosmos account.
        private readonly string PrimaryKey;

        // The Cosmos client instance
        private CosmosClient _cosmosClient;

        // The database we will create
        private Database _database;

        // The container we will create.
        private Container _container;

        // The name of the database and container we will create
        private string databaseId = "tracking";
        private string containerId = "dailyTracked";

        public CosmosRepository()
        {
            EndpointUri = Environment.GetEnvironmentVariable("endpointUri");
            PrimaryKey = Environment.GetEnvironmentVariable("primaryKey");
            _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
        }

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        public async Task CreateDatabaseAsync()
        {
            // Create a new database
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", _database.Id);
        }
        // </CreateDatabaseAsync>

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/partitionKey" as the partition key path since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        public async Task CreateContainerAsync()
        {
            // Create a new container
            _container = await _database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            Console.WriteLine("Created Container: {0}\n", _container.Id);
        }
        // </CreateContainerAsync>

        public async Task<ItemResponse<GymDayTracker>> GetItemAsync(string partitionKey, string id)
        {
            return await _container.ReadItemAsync<GymDayTracker>(id, new PartitionKey(partitionKey));
        }

        public async Task UpsertItemAsync(GymDayTracker gymDayTracker)
        {
            await _container.UpsertItemAsync(gymDayTracker);
        }

        // <AddItemsToContainerAsync>
        public async Task AddItemsToContainerAsync(GymDayTracker gymDayTracker)
        {
            bool itemExists = await DoesItemExistAsync(gymDayTracker.PartitionKey, gymDayTracker.Id);
            
            if (itemExists)
            {
                Console.WriteLine("Item in database with id: {0} already exists\n");
            }
            else
            {
                ItemResponse<GymDayTracker> gymDayTrackerResponse = await _container.CreateItemAsync<GymDayTracker>(gymDayTracker, new PartitionKey(gymDayTracker.PartitionKey));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", gymDayTrackerResponse.Resource.Id, gymDayTrackerResponse.RequestCharge);
            }
        }

        public async Task<bool> DoesItemExistAsync(string partitionKey, string id)
        {
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<GymDayTracker> gymDayTrackerResponse = await _container.ReadItemAsync<GymDayTracker>(id, new PartitionKey(partitionKey));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
        // </AddItemsToContainerAsync>

        //// <QueryItemsAsync>
        ///// <summary>
        ///// Run a query (using Azure Cosmos DB SQL syntax) against the container
        ///// Including the partition key value of lastName in the WHERE filter results in a more efficient query
        ///// </summary>
        //private async Task QueryItemsAsync()
        //{
        //    var sqlQueryText = "SELECT * FROM c WHERE c.PartitionKey = 'Andersen'";

        //    Console.WriteLine("Running query: {0}\n", sqlQueryText);

        //    QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
        //    FeedIterator<GymDayTracker> queryResultSetIterator = this.container.GetItemQueryIterator<GymDayTracker>(queryDefinition);

        //    List<GymDayTracker> families = new List<GymDayTracker>();

        //    while (queryResultSetIterator.HasMoreResults)
        //    {
        //        FeedResponse<GymDayTracker> currentResultSet = await queryResultSetIterator.ReadNextAsync();
        //        foreach (GymDayTracker family in currentResultSet)
        //        {
        //            families.Add(family);
        //            Console.WriteLine("\tRead {0}\n", family);
        //        }
        //    }
        //}
        // </QueryItemsAsync>

        // </ReplaceFamilyItemAsync>

        // <DeleteFamilyItemAsync>
        /// <summary>
        /// Delete an item in the container
        /// </summary>
        public async Task DeleteFamilyItemAsync(string partitionKey, string id)
        {
            ItemResponse<GymDayTracker> deletedResponse = await _container.DeleteItemAsync<GymDayTracker>(id,new PartitionKey(partitionKey));
            Console.WriteLine("Deleted Family [{0},{1}]\n", partitionKey, id);
        }
    }
}
