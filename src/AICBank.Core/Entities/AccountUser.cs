using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AICBank.Core.Entities
{
    public class AccountUser : Entity
    {
        public string Name { get; set; }
        public string IdentityUserId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public BankAccount BankAccount { get; set; }
        public int BankAccountId { get; set; }
    }
}