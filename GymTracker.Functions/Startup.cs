using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosGettingStartedTutorial;
using GymTracker.Domain;
using GymTracker.Domain.Interfaces;
using GymTracker.Domain.Services;
using GymTracker.Repository;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(GymTracker.Functions.Startup))]
namespace GymTracker.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IAzureRepository, AzureRepository>();
            builder.Services.AddScoped<ITrackingService, TrackingService>();
            builder.Services.AddScoped<ICosmosRepository, CosmosRepository>();
        }
    }
}