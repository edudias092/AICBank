using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;
using AICBank.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AICBank.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ILogger<BankAccountController> _logger;
    public BankAccountController(IBankAccountService bankAccountService, ILogger<BankAccountController> logger)
    {
        _bankAccountService = bankAccountService;
        _logger = logger;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _bankAccountService.GetBankAccountById(id);

            if(result == null || !result.Success)
            {
                return NotFound();
            }

            return Ok(result);
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

    [HttpPost]
    public async Task<IActionResult> Create(BankAccountDTO bankAccountDto)
    {
        try
        {
            var result = await _bankAccountService.CreateBankAccount(bankAccountDto);

            if(!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Created("", new { id = result.Data?.Id});
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BankAccountDTO bankAccountDto)
    {
        try
        {
            var result = await _bankAccountService.UpdateBankAccount(bankAccountDto);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);

            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Erro inesperado");

            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado");
        }
    }

    [HttpPost("{id:int}/documents")]
    public async Task<IActionResult> SendMandatoryDocuments([FromRoute] int id, [FromForm] MandatoryDocumentsDTO mandatoryDocumentsDto)
    {
        try
        {
            var result = await _bankAccountService.SendMandatoryDocuments(id, mandatoryDocumentsDto);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);

            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Erro inesperado");

            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado");
        }
    }

    [HttpGet("accountuser/{id:int}")]
    public async Task<IActionResult> GetByAccountUser(int id)
    {
        try
        {
            var result = await _bankAccountService.GetBankAccountByAccountUserId(id);

            if(result == null || !result.Success)
            {
                return NotFound();
            }

            return Ok(result);
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

    [HttpGet("{id:int}/movements")]
    public async Task<IActionResult> GetMovements(int id, [FromQuery]DateTime initialDate, [FromQuery]DateTime finalDate)
    {
        try
        {
            var result = await _bankAccountService.GetMovements(id, initialDate, finalDate);

            if(result == null || !result.Success)
            {
                return NotFound();
            }

            return Ok(result);
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

    [HttpPost("{id:int}/charge")]
    public async Task<IActionResult> CreateCharge(int id, ChargeDTO chargeDto)
    {
        ResponseDTO<CelcashChargeDTO> result;
        try
        {
            result = await _bankAccountService.CreateCharge(id, chargeDto);

            if(!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch(InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);

            return BadRequest(new ResponseDTO<ChargeDTO>
            {
                Errors= [ex.Message]
            });
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex, "Erro inesperado");

            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO<ChargeDTO>
            {
                Errors= ["Ocorreu um erro inesperado."]
            });
        }
    }

    [HttpGet("{id:int}/charges")]
    public async Task<IActionResult> GetCharges(int id, [FromQuery]DateTime? initialDate, [FromQuery]DateTime? finalDate)
    {
        try
        {
            var result = await _bankAccountService.GetCharges(id, initialDate, finalDate);

            if(result == null || !result.Success)
            {
                return NotFound(string.Join(",", result.Errors));
            }

            return Ok(result);
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

    [HttpGet("{id:int}/charges/{chargeId}")]
    public async Task<IActionResult> GetCharge(int id, string chargeId)
    {
        try
        {
            var result = await _bankAccountService.GetChargeById(id, chargeId);

            if(result == null || !result.Success)
            {
                return NotFound(string.Join(",", result.Errors));
            }

            return Ok(result);
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
    
    [HttpDelete("{id:int}/charges/{chargeId}")]
    public async Task<IActionResult> CancelCharge(int id, string chargeId)
    {
        try
        {
            var result = await _bankAccountService.CancelCharge(id, chargeId);

            if(result is not { Success: true })
            {
                return NotFound(string.Join(",", result.Errors));
            }

            return Ok(result);
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

    [HttpGet("{id:int}/balance")]
    public async Task<IActionResult> GetBalance(int id)
    {
        try
        {
            var result = await _bankAccountService.GetBalance(id);

            if(result is not { Success: true })
            {
                return NotFound(string.Join(",", result.Errors));
            }

            return Ok(result);
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
    
    [HttpPost("{id:int}/payment")]
    public async Task<IActionResult> MakePayment(int id, CelcashPaymentRequestDto paymentRequest)
    {
        ResponseDTO<CelcashPaymentResponseDto> result;
        try
        {
            result = await _bankAccountService.MakePayment(id, paymentRequest);

            if(!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch(InvalidOperationException ex)
        {
            _logger.LogError(ex.Message);

            return BadRequest(new ResponseDTO<ChargeDTO>
            {
                Errors= [ex.Message]
            });
        }
        catch(Exception ex)
        {
            _logger.LogCritical(ex, "Erro inesperado");

            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseDTO<ChargeDTO>
            {
                Errors= ["Ocorreu um erro inesperado."]
            });
        }
    }
    
    [HttpGet("{id:int}/charges/sumByDate")]
    public async Task<IActionResult> GetChargesSumByDate(int id)
    {
        try
        {
            var result = await _bankAccountService.GetChargesGroup(id);

            if(result is not { Success: true })
            {
                return NotFound(string.Join(",", result.Errors));
            }

            return Ok(result);
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
