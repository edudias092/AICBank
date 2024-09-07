using AICBank.Core.DTOs;
using AICBank.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AICBank.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ICelCashClientService _ccService;
    public BankAccountController(IBankAccountService bankAccountService, ICelCashClientService ccService)
    {
        _bankAccountService = bankAccountService;
        _ccService = ccService;
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
            //TODO: log the error.
            return BadRequest(ex.Message);
        }
        catch(Exception ex)
        {
            //TODO: log the error.
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
    
    [HttpPost("integrate/{id:int}")]
    public async Task<IActionResult> Integrate(int id)
    {
        try
        {
            var result = await _bankAccountService.IntegrateBankAccount(id);

            if(!result.Success)
            {
                return BadRequest(result.Errors);
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
}
