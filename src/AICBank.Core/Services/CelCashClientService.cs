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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AICBank.Core.Services;

public class CelCashClientService : ICelCashClientService
{
    private readonly IConfiguration _config;
    private HttpClient _httpClient;
    private readonly string _mainGalaxId;
    private readonly string _mainGalaxHash;
    private JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<CelCashClientService> _logger;
    public CelCashClientService(IConfiguration config, IHttpClientFactory _httpClientFactory, ILogger<CelCashClientService> logger)
    {
        _config = config;
        _httpClient = _httpClientFactory.CreateClient("CelCashHttpClient");
        _logger = logger;

        string baseCelCashUrl = _config.GetSection("CelCash").GetValue<string>("baseUrl");
        _httpClient.BaseAddress = new Uri(baseCelCashUrl);

        _mainGalaxId = _config.GetSection("CelCash").GetValue<string>("galaxId") 
                        ?? throw new InvalidOperationException("galaxId não encontrado.");
        _mainGalaxHash = _config.GetSection("CelCash").GetValue<string>("galaxHash")
                        ?? throw new InvalidOperationException("galaxHash não encontrado.");

        _jsonSerializerOptions = new JsonSerializerOptions{
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
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
            var contentString = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro ao obter o token. Response Content: {0}", contentString);

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

    public async Task<CelcashSubaccountResponseDTO> SendMandatoryDocuments(CelcashSendMandatoryDocumentsDTO sendMandatoryDocumentsDTO, BankAccountDTO bankAccountDTO)
    {
        var token = await CreateAuthToken(bankAccountDTO.GalaxId, bankAccountDTO.GalaxHash, ["company.write", "company.read"]);

        using var request = new HttpRequestMessage(HttpMethod.Post, "company/mandatory-documents");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(sendMandatoryDocumentsDTO, null, _jsonSerializerOptions);

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
        
        using var request = new HttpRequestMessage(HttpMethod.Get, string.Format("company/balance/movements{0}", query));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.SendAsync(request);

        if(response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<BankStatementDTO>(content, _jsonSerializerOptions);

            return data;
        }
        else{
            var contentError = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<BankStatementDTO>(contentError, _jsonSerializerOptions);

            return data;
        }
    }

    public async Task<CelcashChargeResponseDTO> CreateCharge(BankAccountDTO bankAccountDTO, ChargeDTO chargeDTO)
    {
        //TODO: remover o main e deixar o da conta bancária.
        var token = await CreateAuthToken(_mainGalaxId, _mainGalaxHash, ["charges.write"]);

        using var request = new HttpRequestMessage(HttpMethod.Post, "charges");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(chargeDTO, null, _jsonSerializerOptions);

        var response = await _httpClient.SendAsync(request);

        if(response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<CelcashChargeResponseDTO>(content, _jsonSerializerOptions);

            return data;
        }
        else{
            var contentError = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<CelcashChargeResponseDTO>(contentError, _jsonSerializerOptions);

            return data;
        }
    }

    public async Task<CelcashChargeDTO[]> GetCharges(BankAccountDTO bankAccountDTO, DateTime? initialDate, DateTime? finalDate)
    {
        //TODO: remover o main e deixar o da conta bancária.
        var token = await CreateAuthToken(_mainGalaxId, _mainGalaxHash, ["charges.read"]);
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
        using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.SendAsync(request);

        if(response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug(content);

            var data = JsonSerializer.Deserialize<JsonObject>(content, _jsonSerializerOptions);

            var charges = JsonSerializer.Deserialize<CelcashChargeDTO[]>(data["Charges"], _jsonSerializerOptions);

            return charges;
        }
        else{
            var contentError = await response.Content.ReadAsStringAsync();
            _logger.LogDebug(contentError);

            var data = JsonSerializer.Deserialize<JsonObject>(contentError, _jsonSerializerOptions);

            return null;
        }
    }

    public async Task<CelcashChargeDTO> GetChargeById(BankAccountDTO bankAccountDTO, string chargeId)
    {
        //TODO: remover o main e deixar o da conta bancária.
        var token = await CreateAuthToken(_mainGalaxId, _mainGalaxHash, ["charges.read"]);
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

        using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.SendAsync(request);

        if(response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug(content);

            var data = JsonSerializer.Deserialize<JsonObject>(content, _jsonSerializerOptions);

            var charges = JsonSerializer.Deserialize<CelcashChargeDTO[]>(data["Charges"], _jsonSerializerOptions);

            return charges.FirstOrDefault();
        }
        else{
            var contentError = await response.Content.ReadAsStringAsync();
            _logger.LogDebug(contentError);

            var data = JsonSerializer.Deserialize<JsonObject>(contentError, _jsonSerializerOptions);

            return null;
        }
    }

}
