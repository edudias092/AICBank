using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Interfaces;
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
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(ICelCashClientService celCashClientService, ILogger<ConfigController> logger)
    {
        _celCashClientService = celCashClientService;
        _logger = logger;
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

            return Ok(new ResponseDTO<CelcashListSubaccountResponseDto>()
            {
                Success = true,
                Data = subaccountList,
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
}