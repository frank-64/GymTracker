using GymTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Interfaces
{
    public interface ITrackingService
    {
        public Task ManageInflux(int amount);
        public Task ManageOutflow(int amount);
        public Task<Occupancy> GetCurrentOccupancy();
    }
}
