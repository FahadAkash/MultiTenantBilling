using Microsoft.AspNetCore.Mvc;
using MultiTenantBilling.Api.Attributes;
using MultiTenantBilling.Api.Services;
using MultiTenantBilling.Application.DTOs;
using MultiTenantBilling.Application.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireTenant]
    public class UserController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ITenantService _tenantService;

        public UserController(
            IAuthorizationService authorizationService,
            ITenantService tenantService)
        {
            _authorizationService = authorizationService;
            _tenantService = tenantService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            // In a real implementation, you would get the user from the JWT token
            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            return Ok(userDto);
        }

        [HttpGet("permissions")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions()
        {
            // In a real implementation, you would get the user email from the JWT token
            var permissions = await _authorizationService.GetUserPermissionsAsync("user@example.com");
            return Ok(permissions);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles()
        {
            // In a real implementation, you would get the user email from the JWT token
            var roles = await _authorizationService.GetUserRolesAsync("user@example.com");
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