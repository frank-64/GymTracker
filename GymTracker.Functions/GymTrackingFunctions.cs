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
using GymTracker.Domain.Entities;
using System.Net.Http;

namespace GymTracker.Functions
{
    public class GymTrackingFunctions
    {
        private readonly ITrackingService _trackingService;
        private readonly IGymDetailsService _gymDetailsService;

        public GymTrackingFunctions(ITrackingService trackingService, IGymDetailsService gymDetailsService)
        {
            _trackingService = trackingService;
            _gymDetailsService = gymDetailsService;
        }

        [FunctionName("GetGymDetails")]
        public async Task<IActionResult> GetGymDetails(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getGymDetails")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var gymDetails = await _gymDetailsService.GetGymDetails();
            var jsonGymDetails = JsonConvert.SerializeObject(gymDetails);
            return new OkObjectResult(jsonGymDetails);
        }

        [FunctionName("UpdateGymDetails")]
        public async Task<IActionResult> UpdateGymDetails(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "updateGymDetails")] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GymDetails updatedGymDetails = JsonConvert.DeserializeObject<GymDetails>(requestBody);

            await _gymDetailsService.UpdateGymDetails(updatedGymDetails);
            return new OkResult();
        }

        [FunctionName("AdminLogin")]
        public async Task<IActionResult> AdminLogin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "determineAdminLogin")] HttpRequest req,
        ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Credentials credentials = JsonConvert.DeserializeObject<Credentials>(requestBody);

            log.LogInformation($"C# HTTP trigger function processed a request for username {credentials.Username}.");
            bool loginResult = false;
            try
            {
                loginResult = await _gymDetailsService.AdminLogin(credentials);
            }
            catch(Exception e)
            {
                log.LogError(e.Message, "An error occurred when accessing the KeyVault");
                return new BadRequestObjectResult("Your username was not valid.");
            }

            if (loginResult)
            {
                return new OkResult();
            }
            log.LogInformation($"The password did not match the hashed password stored.");
            return new BadRequestObjectResult("The password you provided was not correct.");
        }


        [FunctionName("DetermineGymOccupancy")]
        public async Task<IActionResult> DetermineGymOccupancy(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "determineGymOccupancy")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var occupancy = await _trackingService.GetCurrentOccupancy();
            var jsonOccupancy = JsonConvert.SerializeObject(occupancy);
            return new OkObjectResult(jsonOccupancy);
        }

        [FunctionName("InfluxOccurrence")]
        public async Task<IActionResult> InfluxOccurrence(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "influxOccurance")] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string influxAmount = req.Query["amount"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            influxAmount = influxAmount ?? data?.amount;

            log.LogInformation($"An influx of {influxAmount} person(s) has entered the gym. Attempting to update overall occupancy.");
            await _trackingService.ManageInflux(int.Parse(influxAmount));

            string responseMessage = string.IsNullOrEmpty(influxAmount)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"An influx of {influxAmount} person(s) entering the gym was recorded successfully..";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("OutflowOccurrence")]
        public async Task<IActionResult> OutflowOccurrence(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "outflowOccurance")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string outflowAmount = req.Query["amount"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            outflowAmount = outflowAmount ?? data?.amount;

            log.LogInformation($"An outflow of {outflowAmount} person(s) has left the gym. Attempting to update overall occupancy.");
            await _trackingService.ManageOutflow(int.Parse(outflowAmount));

            string responseMessage = string.IsNullOrEmpty(outflowAmount)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"An outflow of {outflowAmount} person(s) leaving the gym was recorded successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("OccupancyEvent")]
        public async Task<IActionResult> OccupancyEvent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "occupancyEvent")] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation(requestBody);
            log.LogInformation($"Event occurred with body: {data}");

            return new OkObjectResult(data);
        }
    }
}
