using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AICBank.Core.Entities
{
    public class LegalBankAccount : BankAccount
    {
        public string NameDisplay { get; set; }
        public string ResponsibleDocument { get; set; }
        public string TypeCompany { get; set; }
        public string Cnae { get; set; }
    }
}