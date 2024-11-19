using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AICBank.Core.Util;

public class HttpRequestSender
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<HttpRequestSender> _logger;
    
    public HttpRequestSender(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions, ILogger<HttpRequestSender> logger)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions;
        _logger = logger;
    }
    
    public async Task<T> SendAsync<T>(HttpRequestMessage request, Func<string, T> handler = null)
    {
        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            return handler == null ? JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions) : handler(content);
        }

        var contentError = await response.Content.ReadAsStringAsync();
        
        _logger.LogError(contentError);

        return default;
    }
}