using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    public class UserController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IApiTenantService _tenantService;

        public UserController(
            IAuthorizationService authorizationService,
            IApiTenantService tenantService)
        {
            _authorizationService = authorizationService;
            _tenantService = tenantService;
        }

        [HttpGet("me")]
        public ActionResult<UserDto> GetCurrentUser()
        {
            // Get the user email from the JWT token
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            // In a real implementation, you would get the full user information from a service
            var userDto = new UserDto
            {
                Id = System.Guid.NewGuid(),
                Email = userEmail,
                FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "",
                LastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? "",
                IsActive = true
            };

            return Ok(userDto);
        }

        [HttpGet("permissions")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions()
        {
            // Get the user email from the JWT token
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var permissions = await _authorizationService.GetUserPermissionsAsync(userEmail);
            return Ok(permissions);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles()
        {
            // Get the user email from the JWT token
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var roles = await _authorizationService.GetUserRolesAsync(userEmail);
            return Ok(roles);
        }

        [HttpPost("assign-role")]
        public async Task<ActionResult<bool>> AssignRole([FromBody] RoleAssignmentDto roleAssignmentDto)
        {
            var result = await _authorizationService.AssignRoleAsync(roleAssignmentDto.UserEmail, roleAssignmentDto.RoleName);
            return Ok(result);
        }
    }

    public class RoleAssignmentDto
    {
        public string UserEmail { get; set; } = default!;
        public string RoleName { get; set; } = default!;
    }
}