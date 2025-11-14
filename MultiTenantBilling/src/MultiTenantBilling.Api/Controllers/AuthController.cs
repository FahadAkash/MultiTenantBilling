using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Controllers
{
    /// <summary>
    /// Authentication controller for user registration, login, and password management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user with the provided details.
        /// </summary>
        /// <param name="registerDto">The registration details.</param>
        /// <returns>An authentication response with a JWT token.</returns>
        /// <response code="200">Returns the authentication response.</response>
        /// <response code="400">If the user already exists or validation fails.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="loginDto">The login credentials.</param>
        /// <returns>An authentication response with a JWT token.</returns>
        /// <response code="200">Returns the authentication response.</response>
        /// <response code="401">If the credentials are invalid.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(typeof(string), 401)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Changes the password for the authenticated user.
        /// </summary>
        /// <param name="changePasswordDto">The current and new passwords.</param>
        /// <returns>True if the password was changed successfully.</returns>
        /// <response code="200">Returns true if successful.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            // Get the user email from the JWT token
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var result = await _authService.ChangePasswordAsync(userEmail, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            return Ok(result);
        }
    }

    /// <summary>
    /// DTO for changing user password.
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// The user's current password.
        /// </summary>
        public string CurrentPassword { get; set; } = default!;

        /// <summary>
        /// The user's new password.
        /// </summary>
        public string NewPassword { get; set; } = default!;
    }
}