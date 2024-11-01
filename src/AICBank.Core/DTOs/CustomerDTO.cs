using System;

namespace AICBank.Core.DTOs;

public class CustomerDTO
{
    public string Name { get; set; }
    public string Document { get; set; }

    public string Email { get; set; }
    public string Phone { get; set; }
    public string[] Emails { get => [Email]; }
    public string[] Phones { get => [Phone]; }
}
