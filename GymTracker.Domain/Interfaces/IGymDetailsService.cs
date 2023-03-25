using GymTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Interfaces
{
    public interface IGymDetailsService
    {
        public Task<GymDetails> GetGymDetails();
    }
}
