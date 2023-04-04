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
using Azure.Storage.Queues;

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

        [FunctionName("GetGymStatus")]
        public async Task<IActionResult> GetGymStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getGymStatus")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var gymStatus = await _trackingService.GetGymStatusAsync();
            var json = JsonConvert.SerializeObject(gymStatus);
            return new OkObjectResult(json);
        }

        [FunctionName("UpdateGymStatus")]
        public async Task<IActionResult> UpdateGymStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "updateGymStatus")] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GymStatus updatedGymStatus = JsonConvert.DeserializeObject<GymStatus>(requestBody);

            await _trackingService.UpdateGymStatusAsync(updatedGymStatus);
            var json = JsonConvert.SerializeObject(updatedGymStatus);
            return new OkObjectResult(json);
        }


        [FunctionName("GetGymInsights")]
        public async Task<IActionResult> GetGymInsights(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getGymInsights")] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                GymInsightsDTO gymInsights = await _trackingService.GetGymInsightsAsync();
                var json = JsonConvert.SerializeObject(gymInsights);
                return new OkObjectResult(json);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [FunctionName("ExpireGymEntryValue")]
        public async Task ExpireGymEntryValue([QueueTrigger("gym-occupancy", Connection = "storageAccountConnStr")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            log.LogInformation($"A person has left the gym. Attempting to update overall occupancy.");
            try
            {
                await _trackingService.DecrementCountAsync(1);
            }
            catch
            {
                log.LogError("An error occurred reducing the occupancy count");
            }
        }

        //TODO: Uncomment when entry/exit events are setup
        //[FunctionName("UpdateDailyGymInsights")]
        //public async Task UpdateDailyGymInsights([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log) // This will run every hour on the hour.
        //{
        //    log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        //    await _trackingService.UpdateHourlyGymInsightsAsync();
        //}

        [FunctionName("GymEntryEvent")]
        public async Task<IActionResult> GymEntryEvent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gymEntryEvent")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a Kisi WebHook request for a GymEntryEvent");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation(requestBody);
            log.LogInformation($"Entry event occurred with body: {data}");

            // TODO: Do logging on the event to ensure idempotency!

            log.LogInformation($"A person has left the gym. Attempting to update overall occupancy.");
            try
            {
                await _trackingService.IncrementCountAsync(1);
                //await gymOccupancyQueue.SendMessageAsync(DateTime.Now.ToString(), visibilityTimeout: TimeSpan.FromMinutes(45));
            }
            catch
            {
                log.LogError("An error occurred handling a gym entry event triggered by the Kisi WebHook.");
            }
            return new OkObjectResult("Occupancy was successfully updated to record 1 person entering the gym.");
        }

        [FunctionName("GymExitEvent")]
        public async Task<IActionResult> GymExitEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gymExitEvent")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a Kisi WebHook request for a GymExitEvent");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            log.LogInformation(requestBody);
            log.LogInformation($"Exit event occurred with body: {data}");

            // TODO: Do logging on the event to ensure idempotency!

            log.LogInformation($"A person has left the gym. Attempting to update overall occupancy.");
            try
            {
                await _trackingService.DecrementCountAsync(1);
            }catch
            {
                log.LogError("An error occurred handling a gym exit event triggered by the Kisi WebHook.");
            }
            return new OkObjectResult("Occupancy was successfully updated to record 1 person leaving the gym.");
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
            await _trackingService.IncrementCountAsync(int.Parse(influxAmount));

            string responseMessage = string.IsNullOrEmpty(influxAmount)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"An influx of {influxAmount} person(s) entering the gym was recorded successfully.";

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
            await _trackingService.DecrementCountAsync(int.Parse(outflowAmount));

            string responseMessage = string.IsNullOrEmpty(outflowAmount)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"An outflow of {outflowAmount} person(s) leaving the gym was recorded successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
