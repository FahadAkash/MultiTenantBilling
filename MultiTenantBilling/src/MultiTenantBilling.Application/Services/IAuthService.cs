using MultiTenantBilling.Application.DTOs;
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
    }
}