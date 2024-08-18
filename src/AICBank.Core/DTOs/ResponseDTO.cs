using System;

namespace AICBank.Core.DTOs;

public class ResponseDTO<T> where T : class
{
    public string Action { get; set; }
    public bool Success { get; set; }
    public T Data { get; set; }
    public string[] Errors { get; set; }
}
