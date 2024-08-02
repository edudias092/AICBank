
namespace AICBank.Core.Entities
{
    public class BankAccount : Entity
    {
        public string Name { get; set; }
        public string Document { get; set; }
        public string Phone { get; set; }
        public string EmailContact { get; set; }
        public string Logo { get; set; }
        public string SoftDescriptor { get; set; }
        public Address Address { get; set; }
        public string GalaxPayId { get; set; }
        public string GalaxId { get; set; }
        public string GalaxHash { get; set; }
        public StatusBankAccount Status { get; set; }
    }
}