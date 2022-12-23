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
    public class OutflowOccurrenceFunction
    {
        private readonly ITrackingService _trackingService;

        public OutflowOccurrenceFunction(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        [FunctionName("OutflowOccurrenceFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
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
    }
}
