using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AICBank.Core.DTOs
{
    public class AuthResult
    {
        public string Action { get; set; }
        public string[] Errors { get; set; }
        public bool Success { get; set; }
    }
}