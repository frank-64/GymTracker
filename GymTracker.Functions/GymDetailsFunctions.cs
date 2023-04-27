using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GymTracker.Domain.Entities;
using GymTracker.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace GymTracker.Functions
{
    public class GymDetailsFunctions
    {
        private readonly IGymDetailsService _gymDetailsService;

        public GymDetailsFunctions(IGymDetailsService gymDetailsService)
        {
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
            }
            catch (Exception ex)
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
                loginResult = await _gymDetailsService.AdminLoginAsync(credentials);
            }
            catch (Exception e)
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
    }
}