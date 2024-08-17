using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AICBank.Core.DTOs
{
    public class UserToken
    {
        public string Token { get; set; }
        public DateTime ExpiresIn { get; set; }
        public UserClaims[] Claims  { get; set; }
    }

    public class UserClaims 
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}