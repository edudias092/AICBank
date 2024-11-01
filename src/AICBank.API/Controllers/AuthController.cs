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
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
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
                return BadRequest(
                    e.Errors.Select(err => err.ErrorMessage.ToString()).ToArray()
                );
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, "{0} : Erro Inesperado", nameof(Register));

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
                _logger.LogError(ex, ex.Message);

                return BadRequest(ex.Message);
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, "{0} : Erro Inesperado", nameof(Register));

                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
