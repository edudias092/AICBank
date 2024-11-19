using System;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Interfaces;
using AICBank.Core.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AICBank.Core.Services;

public class CelCashClientService : ICelCashClientService
{
    private readonly HttpClient _httpClient;
    private readonly string _mainGalaxId;
    private readonly string _mainGalaxHash;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<CelCashClientService> _logger;
    private readonly HttpRequestSender _httpRequestSender;
    public CelCashClientService(IConfiguration config, 
                                IHttpClientFactory httpClientFactory, 
                                ILogger<CelCashClientService> logger,
                                ILogger<HttpRequestSender> loggerSender)
    {
        _httpClient = httpClientFactory.CreateClient("CelCashHttpClient");
        _logger = logger;

        string baseCelCashUrl = config.GetSection("CelCash").GetValue<string>("baseUrl");
        _httpClient.BaseAddress = new Uri(baseCelCashUrl);

        _mainGalaxId = config.GetSection("CelCash").GetValue<string>("galaxId") 
                        ?? throw new InvalidOperationException("galaxId não encontrado.");
        _mainGalaxHash = config.GetSection("CelCash").GetValue<string>("galaxHash")
                        ?? throw new InvalidOperationException("galaxHash não encontrado.");

        _jsonSerializerOptions = new JsonSerializerOptions{
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        _httpRequestSender = new HttpRequestSender(_httpClient, _jsonSerializerOptions, loggerSender);
    }

    private string GetBase64EncodedToken(string galaxId,string galaxHash)
    {
        var bytes = Encoding.UTF8.GetBytes($"{galaxId}:{galaxHash}");

        return Convert.ToBase64String(bytes);
    }
    
    private async Task<string> CreateAuthToken(string galaxId, string galaxHash, string[] permissions)
    {
        var token = GetBase64EncodedToken(galaxId, galaxHash);
        var content = JsonContent.Create(new CelcashTokenRequestDTO
        {
            GrantType = "authorization_code",
            Scope = string.Join(" ", permissions)
        });
        
        using var request = HttpRequestBuilder.Create("token", HttpMethod.Post)
            .AddAuthorization("Basic", token)
            .AddContent(content)
            .Build();

        var response = await _httpClient.SendAsync(request);

        if(response.IsSuccessStatusCode){
            var contentString = await response.Content.ReadAsStringAsync();
            var convertedReponse = JsonSerializer.Deserialize<CelcashTokenResponseDTO>(
                                                    contentString, _jsonSerializerOptions);

            return convertedReponse.AccessToken;
        }
        else{
            var contentString = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro ao obter o token. Response Content: {0}", contentString);

            throw new InvalidOperationException("Erro ao obter o token");
        }
    }

    public async Task<CelcashSubaccountResponseDTO> CreateSubBankAccount(BankAccountDTO bankAccountDto)
    {
        var token = await CreateAuthToken(_mainGalaxId, _mainGalaxHash, ["company.write", "company.read"]);

        using var request = HttpRequestBuilder.Create("company/subaccount", HttpMethod.Post)
            .AddAuthorization("Bearer", token)
            .AddContent(JsonContent.Create(bankAccountDto, null, _jsonSerializerOptions))
            .Build();
        
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

    public async Task<CelcashSubaccountResponseDTO> SendMandatoryDocuments(CelcashSendMandatoryDocumentsDTO sendMandatoryDocumentsDTO, BankAccountDTO bankAccountDTO)
    {
        var token = await CreateAuthToken(bankAccountDTO.GalaxId, 
                                bankAccountDTO.GalaxHash, 
                                ["company.write", "company.read"]);

        using var request = HttpRequestBuilder.Create("company/mandatory-documents", HttpMethod.Post)
            .AddAuthorization("Bearer", token)
            .AddContent(JsonContent.Create(sendMandatoryDocumentsDTO, null, _jsonSerializerOptions))
            .Build();

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

    public async Task<BankStatementDTO> Movements(BankAccountDTO bankAccountDTO, DateTime initialDate, DateTime finalDate)
    {
        
        var token = await CreateAuthToken(bankAccountDTO.GalaxId, bankAccountDTO.GalaxHash, ["balance.read"]);

        string query = $"?initialDate={initialDate:yyyy-MM-dd}&finalDate={finalDate:yyyy-MM-dd}";

        using var request = HttpRequestBuilder.Create(string.Format("company/balance/movements{0}", query), HttpMethod.Get)
            .AddAuthorization("Bearer", token)
            .Build();
        
        return await _httpRequestSender.SendAsync<BankStatementDTO>(request);
    }

    public async Task<CelcashChargeResponseDTO> CreateCharge(BankAccountDTO bankAccountDto, ChargeDTO chargeDto)
    {
        //TODO: remover o main e deixar o da conta bancária.
        var token = await CreateAuthToken(bankAccountDto.GalaxId, bankAccountDto.GalaxHash, ["charges.write"]);

        using var request = HttpRequestBuilder.Create("charges", HttpMethod.Post)
            .AddAuthorization("Bearer", token)
            .AddContent(JsonContent.Create(chargeDto, null, _jsonSerializerOptions))
            .Build();

        return await _httpRequestSender.SendAsync<CelcashChargeResponseDTO>(request);
    }

    public async Task<CelcashListChargeResponseDTO> GetCharges(BankAccountDTO bankAccountDTO, DateTime? initialDate,
        DateTime? finalDate)
    {
        //TODO: remover o main e deixar o da conta bancária.
        var token = await CreateAuthToken(bankAccountDTO.GalaxId, bankAccountDTO.GalaxHash, ["charges.read"]);
        var uriBuilder = new UriBuilder(_httpClient.BaseAddress) {Path = _httpClient.BaseAddress.AbsolutePath+"charges"};
        var parameters = new Dictionary<string, string>();

        if(initialDate.HasValue) 
            parameters.Add("createdOrUpdatedAtFrom", initialDate.GetValueOrDefault().ToString("yyyy-MM-dd"));
        if(finalDate.HasValue) 
            parameters.Add("createdOrUpdatedAtTo", finalDate.GetValueOrDefault().ToString("yyyy-MM-dd"));

        parameters.Add("startAt", "0");
        parameters.Add("limit", "100");
        parameters.Add("order", "createdAt.desc");

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach(var p in parameters){
            query[p.Key] = p.Value;
        }

        uriBuilder.Query = query.ToString();
        
        using var request = HttpRequestBuilder.Create(uriBuilder.Uri, HttpMethod.Get)
            .AddAuthorization("Bearer", token)
            .Build();

        return await _httpRequestSender.SendAsync<CelcashListChargeResponseDTO>(request);
    }

    public async Task<CelcashListChargeResponseDTO> GetChargeById(BankAccountDTO bankAccountDTO, string chargeId)
    {
        //TODO: remover o main e deixar o da conta bancária.
        var token = await CreateAuthToken(bankAccountDTO.GalaxId, bankAccountDTO.GalaxHash, ["charges.read"]);
        var uriBuilder = new UriBuilder(_httpClient.BaseAddress) {Path = _httpClient.BaseAddress.AbsolutePath+"charges"};
        var parameters = new Dictionary<string, string>
        {
            { "startAt", "0" },
            { "limit", "1" },
            { "order", "createdAt.desc" },
            { "myIds", chargeId }
        };

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach(var p in parameters){
            query[p.Key] = p.Value;
        }

        uriBuilder.Query = query.ToString();

        using var request = HttpRequestBuilder.Create(uriBuilder.Uri, HttpMethod.Get)
            .AddAuthorization("Bearer", token)
            .Build();

        return await _httpRequestSender.SendAsync<CelcashListChargeResponseDTO>(request);
    }

    public async Task<bool> CancelCharge(BankAccountDTO bankAccountDTO, string chargeId)
    {
        var token = await CreateAuthToken(bankAccountDTO.GalaxId, bankAccountDTO.GalaxHash, ["charges.write"]);
        using var request = HttpRequestBuilder.Create($"charges/{chargeId}/myId", HttpMethod.Delete)
            .AddAuthorization("Bearer", token)
            .Build();
        
        return await _httpRequestSender.SendAsync<bool>(request, (content) =>
        {
            var jsonContent = JsonSerializer.Deserialize<JsonObject>(content, _jsonSerializerOptions);

            return jsonContent["type"]!.GetValue<bool>();
        });
    }
}