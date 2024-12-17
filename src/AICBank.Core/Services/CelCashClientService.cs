using System;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Web;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Interfaces;
using AICBank.Core.Util;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;

    private string TokenFormat = "{0}-{1}";
    
    public CelCashClientService(IConfiguration config, 
                                IHttpClientFactory httpClientFactory, 
                                ILogger<CelCashClientService> logger,
                                ILogger<HttpRequestSender> loggerSender,
                                IMemoryCache cache)
    {
        _httpClient = httpClientFactory.CreateClient("CelCashHttpClient");
        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("AicBank", Assembly.GetExecutingAssembly().GetName().Version.ToString())
        );
        
        _logger = logger;

        string baseCelCashUrl = config.GetSection("CelCash").GetValue<string>("baseUrl");
        _httpClient.BaseAddress = new Uri(baseCelCashUrl);

        _mainGalaxId = config.GetSection("CelCash").GetValue<string>("galaxId") 
                        ?? throw new InvalidOperationException("galaxId não encontrado.");
        _mainGalaxHash = config.GetSection("CelCash").GetValue<string>("galaxHash")
                        ?? throw new InvalidOperationException("galaxHash não encontrado.");

        _jsonSerializerOptions = new JsonSerializerOptions{
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        _httpRequestSender = new HttpRequestSender(_httpClient, _jsonSerializerOptions, loggerSender);
        
        _cache = cache;
    }

    private string GetBase64EncodedToken(string galaxId,string galaxHash)
    {
        var bytes = Encoding.UTF8.GetBytes($"{galaxId}:{galaxHash}");

        return Convert.ToBase64String(bytes);
    }
    
    private async Task<string> CreateAuthToken(string galaxId, string galaxHash, string[] permissions)
    {
        var tokenInCache = _cache.Get(string.Format(TokenFormat, galaxId, string.Join("|", permissions)));

        if (tokenInCache != null)
            return tokenInCache.ToString();
        
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

            var cacheKey = string.Format(TokenFormat, galaxId, string.Join("|", permissions));
            _cache.Set(cacheKey, convertedReponse.AccessToken,
                TimeSpan.FromSeconds(convertedReponse.ExpiresIn));
            
            return convertedReponse.AccessToken;
        }
        else{
            var contentString = await response.Content.ReadAsStringAsync();
            
            _logger.LogDebug(contentString);

            _logger.LogError("Erro ao obter o token. Response Content: {0}", contentString);

            throw new InvalidOperationException("Erro ao obter o token");
        }
    }

    public async Task<CelcashCreatedSubaccountResponseDTO> CreateSubBankAccount(BankAccountDTO bankAccountDto)
    {
        var token = await CreateAuthToken(_mainGalaxId, _mainGalaxHash, ["company.write", "company.read"]);
        
        bankAccountDto.Logo = await ImageHelper.GetBase64LogoImage();
        
        using var request = HttpRequestBuilder.Create("company/subaccount", HttpMethod.Post)
            .AddAuthorization("Bearer", token)
            .AddContent(JsonContent.Create(bankAccountDto, null, _jsonSerializerOptions))
            .Build();

        return await _httpRequestSender.SendAsync(request, 
            null, 
            (content) => JsonSerializer.Deserialize<CelcashCreatedSubaccountResponseDTO>(content, _jsonSerializerOptions));
    }

    public async Task<CelcashCreatedSubaccountResponseDTO> SendMandatoryDocuments(CelcashSendMandatoryDocumentsDTO sendMandatoryDocumentsDto, BankAccountDTO bankAccountDto)
    {
        var token = await CreateAuthToken(bankAccountDto.GalaxId, 
                                bankAccountDto.GalaxHash, 
                                ["company.write", "company.read"]);

        using var request = HttpRequestBuilder.Create("company/mandatory-documents", HttpMethod.Post)
            .AddAuthorization("Bearer", token)
            .AddContent(JsonContent.Create(sendMandatoryDocumentsDto, null, _jsonSerializerOptions))
            .Build();

        return await _httpRequestSender.SendAsync<CelcashCreatedSubaccountResponseDTO>(request,
            null,
            (content) => JsonSerializer.Deserialize<CelcashCreatedSubaccountResponseDTO>(content, _jsonSerializerOptions));
    }

    public async Task<BankStatementDTO> Movements(BankAccountDTO bankAccountDto, DateTime initialDate, DateTime finalDate)
    {
        var token = await CreateAuthToken(bankAccountDto.GalaxId, bankAccountDto.GalaxHash, ["balance.read"]);

        string query = $"?initialDate={initialDate:yyyy-MM-dd}&finalDate={finalDate:yyyy-MM-dd}";

        using var request = HttpRequestBuilder.Create(string.Format("company/balance/movements{0}", query), HttpMethod.Get)
            .AddAuthorization("Bearer", token)
            .Build();
        
        return await _httpRequestSender.SendAsync<BankStatementDTO>(request);
    }

    public async Task<CelcashChargeResponseDTO> CreateCharge(BankAccountDTO bankAccountDto, ChargeDTO chargeDto)
    {
        var token = await CreateAuthToken(bankAccountDto.GalaxId, bankAccountDto.GalaxHash, ["charges.write"]);

        using var request = HttpRequestBuilder.Create("charges", HttpMethod.Post)
            .AddAuthorization("Bearer", token)
            .AddContent(JsonContent.Create(chargeDto, null, _jsonSerializerOptions))
            .Build();

        return await _httpRequestSender.SendAsync<CelcashChargeResponseDTO>(request);
    }

    public async Task<CelcashListChargeResponseDTO> GetCharges(BankAccountDTO bankAccountDto, DateTime? initialDate,
        DateTime? finalDate)
    {
        var token = await CreateAuthToken(bankAccountDto.GalaxId, bankAccountDto.GalaxHash, ["charges.read"]);
        var uriBuilder = new UriBuilder(_httpClient.BaseAddress!) {Path = _httpClient.BaseAddress.AbsolutePath+"charges"};
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

        uriBuilder.Query = query.ToString()!;
        
        using var request = HttpRequestBuilder.Create(uriBuilder.Uri, HttpMethod.Get)
            .AddAuthorization("Bearer", token)
            .Build();

        return await _httpRequestSender.SendAsync<CelcashListChargeResponseDTO>(request);
    }

    public async Task<CelcashListChargeResponseDTO> GetChargeById(BankAccountDTO bankAccountDto, string chargeId)
    {
        var token = await CreateAuthToken(bankAccountDto.GalaxId, bankAccountDto.GalaxHash, ["charges.read"]);
        var uriBuilder = new UriBuilder(_httpClient.BaseAddress!) {Path = _httpClient.BaseAddress.AbsolutePath+"charges"};
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

        uriBuilder.Query = query.ToString()!;

        using var request = HttpRequestBuilder.Create(uriBuilder.Uri, HttpMethod.Get)
            .AddAuthorization("Bearer", token)
            .Build();

        return await _httpRequestSender.SendAsync<CelcashListChargeResponseDTO>(request);
    }

    public async Task<bool> CancelCharge(BankAccountDTO bankAccountDto, string chargeId)
    {
        var token = await CreateAuthToken(bankAccountDto.GalaxId, bankAccountDto.GalaxHash, ["charges.write"]);
        using var request = HttpRequestBuilder.Create($"charges/{chargeId}/myId", HttpMethod.Delete)
            .AddAuthorization("Bearer", token)
            .Build();
        
        return await _httpRequestSender.SendAsync<bool>(request, (content) =>
        {
            var jsonContent = JsonSerializer.Deserialize<JsonObject>(content, _jsonSerializerOptions);

            return jsonContent["type"]!.GetValue<bool>();
        });
    }

    public async Task<CelcashBalanceResponseDto> GetBalance(BankAccountDTO bankAccountDto)
    {
        var token = await CreateAuthToken(bankAccountDto.GalaxId, bankAccountDto.GalaxHash, ["balance.read"]);

        using var request = HttpRequestBuilder.Create("company/balance", HttpMethod.Get)
            .AddAuthorization("Bearer", token)
            .Build();

        return await _httpRequestSender.SendAsync<CelcashBalanceResponseDto>(request);
    }

    public async Task<CelcashPaymentResponseDto> MakePayment(BankAccountDTO bankAccountDto, CelcashPaymentRequestDto paymentRequest)
    {
        var token = await CreateAuthToken(bankAccountDto.GalaxId, bankAccountDto.GalaxHash, ["balance.write"]);
        var requestBody = JsonContent.Create(paymentRequest, null, _jsonSerializerOptions);

        using var request = HttpRequestBuilder.Create("pix/payment", HttpMethod.Post)
            .AddAuthorization("Bearer", token)
            .AddContent(requestBody)
            .Build();

        return await _httpRequestSender.SendAsync<CelcashPaymentResponseDto>(request);
    }

    public async Task<CelcashListSubaccountResponseDto> GetSubaccountList(
        CelcashFilterSubaccountDto filterSubaccountDto)
    {
        var token = await CreateAuthToken(_mainGalaxId, _mainGalaxHash, ["company.read"]);
        const string cacheKey = "listSubaccounts";
        if (_cache.Get(cacheKey) != null)
        {
            return JsonSerializer
                            .Deserialize<CelcashListSubaccountResponseDto>(_cache.Get("listSubaccounts").ToString(),
                                _jsonSerializerOptions);
        }
        
        var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
        {
            Path = _httpClient.BaseAddress.AbsolutePath+"company/subaccounts"
        };
        
        var parameters = new Dictionary<string, string>
        {
            { "startAt", "0" },
            { "limit", "200" }
        };

        if (filterSubaccountDto != null && filterSubaccountDto.Documents?.Length > 0)
        {
            parameters.Add("documents", string.Join(",", filterSubaccountDto.Documents));
        }

        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach(var p in parameters){
            query[p.Key] = p.Value;
        }

        uriBuilder.Query = query.ToString()!;

        using var request = HttpRequestBuilder.Create(uriBuilder.Uri, HttpMethod.Get)
            .AddAuthorization("Bearer", token)
            .Build();
        
        var result = await _httpRequestSender.SendAsync<CelcashListSubaccountResponseDto>(request);

        if (result != null)
        {
            _cache.Set(cacheKey, JsonSerializer.Serialize(result, _jsonSerializerOptions), TimeSpan.FromMinutes(20));
        }

        return result;
    }
}
