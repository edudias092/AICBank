
namespace AICBank.Core.Entities
{
    public class Professional : Entity
    {
        public string InternalName { get; set; } //lawyer,doctor,accountant,realtor,broker,physicalEducator,physiotherapist,others
        public string Inscription { get; set; }
        public BankAccount BankAccount { get; set; }
        public int BankAccountId { get; set; }
    }
}