namespace AICBank.Core.Entities;

public class MandatoryDocuments : Entity
{
    public string MotherName { get; set; }
    public DateTime BirthDate { get; set; }
    public int MonthlyIncome { get; set; }
    public string About { get; set; }
    public string SocialMediaLink { get; set; }
    public string AssociateDocument { get; set; }
    public string AssociateType { get; set; }
    public string AssociateName { get; set; }
    public int BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; }
}