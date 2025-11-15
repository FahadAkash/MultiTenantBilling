using MultiTenantBilling.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword);
        Task<bool> AssignRoleAsync(string userEmail, string roleName);
        Task<bool> RemoveRoleAsync(string userEmail, string roleName);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> ActivateUserAsync(Guid userId);
        Task<bool> DeactivateUserAsync(Guid userId);
        Task<AuthResponseDto> AdminRegisterAsync(AdminRegisterDto adminRegisterDto);
    }
}