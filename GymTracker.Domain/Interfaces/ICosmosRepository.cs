using GymTracker.Domain.Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Interfaces
{
    public interface ICosmosRepository
    {
        Task CreateDatabaseAsync(string databaseId);
        Task CreateContainerAsync(string containerId, string partitionKeyFormat);
        Task UpsertItemAsync<T>(T item);
        Task<ItemResponse<Item>> GetItemAsync<Item>(string partitionKey, string id);
        Task AddItemsToContainerAsync(GymDayTracker gymDayTracker);
        Task<ItemResponse<Item>> GetItemIfExistAsync<Item>(string partitionKey, string id);
        Task DeleteItemAsync<Item>(string partitionKey, string id);
    }
}
