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
        Task CreateDatabaseAsync();
        Task CreateContainerAsync();
        Task UpsertItemAsync(GymDayTracker gymDayTracker);
        Task<ItemResponse<GymDayTracker>> GetItemAsync(string partitionKey, string id);
        Task AddItemsToContainerAsync(GymDayTracker gymDayTracker);
        Task<bool> DoesItemExistAsync(string partitionKey, string id);
        Task DeleteFamilyItemAsync(string partitionKey, string id);
    }
}
