using System.Text.Json;
using AICBank.Core.Entities;
using AICBank.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AICBank.Core.Util;

public class HttpRequestSender
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<HttpRequestSender> _logger;
    private readonly IIntegrationLogRepository _integrationLogRepository;
    
    public HttpRequestSender(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions, ILogger<HttpRequestSender> logger, IIntegrationLogRepository integrationLogRepository)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions;
        _logger = logger;
        _integrationLogRepository = integrationLogRepository;
    }
    
    public async Task<T> SendAsync<T>(HttpRequestMessage request, Func<string, T> handler = null, Func<string, T> errorHandler = null) where T : new()
    {
        T data = new T();
        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(content);
            
            data = handler == null ? JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions) : handler(content);
        }
        else
        {
            _logger.LogError(content);
        }
        
        if (errorHandler != null)
        {
            data = errorHandler(content);    
        }
        
        //Temp: Better logic for this decision
        if (request.Content != null)
        {
            var requestContent = await request.Content.ReadAsStringAsync();
            var integrationLog = new IntegrationLog
            {
                Action = request.RequestUri!.AbsolutePath,
                RequestData = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(requestContent)),
                ResponseData = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(content)),
                StatusCode = response.StatusCode.ToString()
            };
        
            await _integrationLogRepository.Add(integrationLog);    
        }
        
        return data;
    }
}