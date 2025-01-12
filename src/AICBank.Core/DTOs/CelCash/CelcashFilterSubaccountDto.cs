namespace AICBank.Core.DTOs.CelCash;

public class CelcashFilterSubaccountDto
{
    public string[] GalaxPayIds { get; set; }
    public string[] Documents { get; set; }
    public int Limit { get; set; } = 100;
    public int StartAt { get; set; } = 0;
    public string Order { get; set; } = "createdAt.desc";
}