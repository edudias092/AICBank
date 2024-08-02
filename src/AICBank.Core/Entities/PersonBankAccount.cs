
namespace AICBank.Core.Entities
{
    public class PersonBankAccount : BankAccount
    {
        public Professional Professional { get; set; }
    }

    public class Professional 
    {

        public string InternalName { get; set; } //lawyer,doctor,accountant,realtor,broker,physicalEducator,physiotherapist,others
        public string Inscription { get; set; }
    }
}