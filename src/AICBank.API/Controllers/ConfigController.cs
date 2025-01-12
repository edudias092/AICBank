using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Interfaces;
using AICBank.Core.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AICBank.API.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class ConfigController : ControllerBase
{
    private readonly ICelCashClientService _celCashClientService;
    private readonly IBankAccountService _bankAccountService;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(ICelCashClientService celCashClientService, ILogger<ConfigController> logger, 
        IBankAccountService bankAccountService)
    {
        _celCashClientService = celCashClientService;
        _logger = logger;
        _bankAccountService = bankAccountService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("subaccounts")]
    public async Task<IActionResult> GetSubaccounts ([FromQuery] CelcashFilterSubaccountDto filterSubaccountDto = null)
    {
        try
        {
            var subaccountList = await _celCashClientService.GetSubaccountList(filterSubaccountDto);

            if (!subaccountList.Type)
            {
                return NotFound();
            }

            return Ok(new ResponseDTO<CelcashCompanyDTO[]>()
            {
                Success = true,
                Data = subaccountList?.Subaccounts,
                Errors = []
            });
        }
        catch(InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);

            return BadRequest(ex.Message);
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex, "Erro inesperado");

            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado");
        }
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("subaccounts/{galaxId}/status")]
    public async Task<IActionResult> PatchStatus(string galaxId, [FromBody] bool approve)
    {
        try
        {
            var result = await _bankAccountService.CheckSubaccountStatus(galaxId, approve);
            
            return Ok(new ResponseDTO<bool>()
            {
                Success = result,
                Data = result,
                Errors = []
            });
        }
        catch(InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);

            return BadRequest(ErrorMapper.CreateErrorResponse(ex.Message));
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex, "Erro inesperado");

            return StatusCode(StatusCodes.Status500InternalServerError, ErrorMapper.CreateErrorResponse("Ocorreu um erro inesperado."));
        }
    }
}