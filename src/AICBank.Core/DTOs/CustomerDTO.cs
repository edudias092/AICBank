using System;
using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs;

public class CustomerDTO
{
    public string Name { get; set; }
    public string Document { get; set; }


    public string Email { get => Emails.FirstOrDefault(); }
    public long Phone { get => Phones.FirstOrDefault();}
    public string[] Emails { get; set; }
    public long[] Phones { get; set;}
}
