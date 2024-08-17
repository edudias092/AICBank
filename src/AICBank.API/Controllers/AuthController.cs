using AICBank.Core.DTOs;
using AICBank.Core.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AICBank.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountUserDTO userDTO)
        {
            try
            {
                var result = await _authService.Register(userDTO);

                return Ok(result);
            }
            catch(ValidationException e)
            {
                return BadRequest(new {
                    errors = e.Errors.Select(err => err.ToString())
                });
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
        {
            try
            {
                var result = await _authService.Login(userLoginDTO.Email, userLoginDTO.Password);

                return Ok(result);
            }
            catch(ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
