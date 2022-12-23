using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;

namespace GymTracker.Functions
{

    public class InfluxOccurrenceFunction
    {
        private readonly ITrackingService _trackingService;

        public InfluxOccurrenceFunction(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        [FunctionName("InfluxOccurrenceFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
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
                : $"An influx of {influxAmount} person(s) entering the gym was recorded successfully.";
            
            return new OkObjectResult(responseMessage);
        }
    }
}
