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
using System.Security.Claims;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
            var json = JsonConvert.SerializeObject(updatedGymDetails);
            return new OkObjectResult(json);
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

        [FunctionName("SetCustomOpeningPeriod")]
        public async Task<IActionResult> SetCustomOpeningPeriod(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "setCustomOpeningPeriod")] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Day customOpeningHour = JsonConvert.DeserializeObject<Day>(requestBody);

            try
            {
                await _gymDetailsService.SetCustomOpeningPeriod(customOpeningHour);
            }catch(Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            var json = JsonConvert.SerializeObject(customOpeningHour);
            return new OkObjectResult(json);
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
                log.LogError(e.Message, "An error occurred when accessing the password hash");
                return new BadRequestObjectResult("Your username was not valid.");
            }

            if (loginResult)
            {
                ClaimsIdentity identity = req.HttpContext.User.Identity as ClaimsIdentity;

                // Create a list of claims from the user's identity
                Claim[] claims = identity.Claims.ToArray();

                // Create a JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWTKey"));
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                // Return the token as a response
                return new OkObjectResult(JsonConvert.SerializeObject(new TokenResponse(tokenHandler.WriteToken(token))));
            }
            log.LogInformation($"The password did not match the hashed password stored.");
            return new BadRequestObjectResult("The password you provided was not correct.");
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

        [FunctionName("GymEntryEvent")]
        public async Task<IActionResult> GymEntryEvent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "gymEntryEvent")] HttpRequest req,
        ILogger log)
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
    }
}
