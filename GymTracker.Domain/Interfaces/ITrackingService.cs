using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Interfaces
{
    public interface ITrackingService
    {
        public void ManageInflux(int amount);
        public void ManageOutflow(int amount);
        public Task<int> GetTotalCapacity();
        public Task<int> GetCurrentOccupancy();
    }
}
