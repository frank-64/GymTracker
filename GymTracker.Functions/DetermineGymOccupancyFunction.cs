using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GymTracker.Domain.Interfaces;

namespace GymTracker.Functions
{
    public class DetermineGymOccupancyFunction
    {
        private readonly ITrackingService _trackingService;

        public DetermineGymOccupancyFunction(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        [FunctionName("DetermineGymOccupancyFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var occupancy = await _trackingService.GetCurrentOccupancy();
            string responseMessage = $"The current occupancy at the gym is {occupancy}.";
            return new OkObjectResult(responseMessage);
        }
    }
}
