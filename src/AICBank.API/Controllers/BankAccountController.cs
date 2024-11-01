using AICBank.Core.DTOs;
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
    public async Task<IActionResult> Create(BankAccountDTO bankAccountDTO)
    {
        try
        {
            var result = await _bankAccountService.CreateBankAccount(bankAccountDTO);

            if(!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Created("", new { id = result.Data?.Id});
        }
        catch(InvalidOperationException ex)
        {
            //TODO: log the error.
            return BadRequest(ex.Message);
        }
        catch(Exception ex)
        {
            //TODO: log the error.
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BankAccountDTO bankAccountDTO)
    {
        try
        {
            var result = await _bankAccountService.UpdateBankAccount(bankAccountDTO);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            //TODO: log the error.
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            //TODO: log the error.
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado");
        }
    }

    [HttpPost("{id:int}/documents")]
    public async Task<IActionResult> SendMandatoryDocuments([FromRoute] int id, [FromForm] MandatoryDocumentsDTO mandatoryDocumentsDTO)
    {
        try
        {
            var result = await _bankAccountService.SendMandatoryDocuments(id, mandatoryDocumentsDTO);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            //TODO: log the error.
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            //TODO: log the error.
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
            //TODO: log the error.
            return BadRequest(ex.Message);
        }
        catch(Exception ex)
        {
            //TODO: log the error.
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
}
