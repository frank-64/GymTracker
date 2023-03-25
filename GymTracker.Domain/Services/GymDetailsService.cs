using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GymTracker.Domain.Services
{
    public class GymDetailsService : IGymDetailsService
    {
        private readonly string blobName = "gymDetails.json";
        private readonly IAzureRepository _azureRepository;

        public GymDetailsService(IAzureRepository azureRepository)
        {
            _azureRepository = azureRepository;
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
    }
}
