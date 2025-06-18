using FinTrackHub.Common;
using FinTrackHub.Models.DTOs;
using FinTrackHub.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Logging;

namespace FinTrackHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthRegisterController : BaseController
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthRegisterController> _logger;
        public AuthRegisterController(AuthService authService, ILogger<AuthRegisterController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequestResponse(GetModelErrors());

                var result = await _authService.RegisterUserAsync(model);

                if (!result)
                    return BadRequestResponse("Registration failed!");

                return OkResponse<string>(null, "Registration successful!");
            }
            catch (Exception ex) {

                _logger.LogError(ex, "Login failed due to unexpected error for {Email}", model.Email);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            _logger.LogInformation("Login endpoint hit at {Time}", DateTime.UtcNow);
            try
            {

                if (!ModelState.IsValid)
                    return BadRequestResponse(GetModelErrors());

                var token = await _authService.LoginUserAsync(model);

                if (token == null)
                    return UnauthorizedResponse("Invalid credentials!");

                return OkResponse(token, "Login successful!");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Login failed due to unexpected error for {Email}", model.Email);
                return StatusCode(500, "Internal server error.");
            }
        }

        private string GetModelErrors()
        {
            return string.Join(", ",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }
    }
}

