using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AICBank.Core.Services;

public class CelCashClientService : ICelCashClientService
{
    private readonly IConfiguration _config;
    private HttpClient _httpClient;
    private readonly string _mainGalaxId;
    private readonly string _mainGalaxHash;
    private JsonSerializerOptions _jsonSerializerOptions;
    public CelCashClientService(IConfiguration config, IHttpClientFactory _httpClientFactory)
    {
        _config = config;
        _httpClient = _httpClientFactory.CreateClient("CelCashHttpClient");

        string baseCelCashUrl = _config.GetSection("CelCash").GetValue<string>("baseUrl");
        _httpClient.BaseAddress = new Uri(baseCelCashUrl);

        _mainGalaxId = _config.GetSection("CelCash").GetValue<string>("galaxId") 
                        ?? throw new InvalidOperationException("galaxId não encontrado.");
        _mainGalaxHash = _config.GetSection("CelCash").GetValue<string>("galaxHash")
                        ?? throw new InvalidOperationException("galaxHash não encontrado.");

        _jsonSerializerOptions = new JsonSerializerOptions{
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        };
    }

    private string GetBase64EncodedToken(string galaxId,string galaxHash)
    {
        var bytes = Encoding.UTF8.GetBytes($"{galaxId}:{galaxHash}");

        return Convert.ToBase64String(bytes);
    }
    
    public async Task<string> CreateAuthToken(string galaxId, string galaxHash, string[] permissions)
    {
        var token = GetBase64EncodedToken(galaxId, galaxHash);
        using var request = new HttpRequestMessage(HttpMethod.Post, "token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        request.Content = JsonContent.Create(new CelcashTokenRequestDTO
        {
            GrantType = "authorization_code",
            Scope = string.Join(" ", permissions)
        });

        var response = await _httpClient.SendAsync(request);

        if(response.IsSuccessStatusCode){
            var contentString = await response.Content.ReadAsStringAsync();
            var convertedReponse = JsonSerializer.Deserialize<CelcashTokenResponseDTO>(
                                                    contentString, _jsonSerializerOptions);

            return convertedReponse.AccessToken;
        }
        else{
            throw new InvalidOperationException("Erro ao obter o token");
        }
    }

    public async Task<CelcashSubaccountResponseDTO> CreateSubBankAccount(BankAccountDTO bankAccountDTO)
    {
        var token = await CreateAuthToken(_mainGalaxId, _mainGalaxHash, ["company.write", "company.read"]);

        using var request = new HttpRequestMessage(HttpMethod.Post, "company/subaccount");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(bankAccountDTO, null, _jsonSerializerOptions);

        var response = await _httpClient.SendAsync(request);

        if(response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<CelcashSubaccountResponseDTO>(content, _jsonSerializerOptions);

            return data;
        }
        else{
            var contentError = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<CelcashSubaccountResponseDTO>(contentError, _jsonSerializerOptions);

            return data;
        }
    }

    public Task SendMandatoryDocuments(BankAccountDTO bankAccountDTO)
    {
        throw new NotImplementedException();
    }
}
