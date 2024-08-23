using AICBank.Core.DTOs;
using AICBank.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AICBank.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountService _bankAccountService;
    public BankAccountController(IBankAccountService bankAccountService)
    {
        _bankAccountService = bankAccountService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var bankAccountDTO = await _bankAccountService.GetBankAccountById(id);

            if(bankAccountDTO == null)
            {
                return NotFound();
            }

            return Ok(bankAccountDTO);
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
    public async Task<IActionResult> Create([FromBody] BankAccountDTO bankAccountDTO)
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
}
