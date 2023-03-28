using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTracker.Domain.Entities
{
    public class TokenResponse
    {
        public string Token { get; set; }

        public TokenResponse(string token)
        { 
            Token = token;
        }
    }
}
