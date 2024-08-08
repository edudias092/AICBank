
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
        public int AddressId{ get; set; }
        public int GalaxPayId { get; set; }
        public string GalaxId { get; set; }
        public string GalaxHash { get; set; }
        public string NameDisplay { get; set; }
        public string ResponsibleDocument { get; set; }
        public string TypeCompany { get; set; }
        public string Cnae { get; set; }
        public StatusBankAccount Status { get; set; }
        public TypeBankAccount Type { get; set; }
        public Professional Professional { get; set; }
        public int ProfessionalId { get; set; }
        public AccountUser AccountUser { get; set; }
        public int AccountUserId { get; set; }
    }
}