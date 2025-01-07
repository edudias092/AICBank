namespace AICBank.Core.Entities;

public class IntegrationLog : Entity
{
    public string Action { get; set; }
    public string StatusCode { get; set; }
    public string RequestData { get; set; }
    public string ResponseData { get; set; }
    public DateTime TimeStamp { get; protected set; }
}