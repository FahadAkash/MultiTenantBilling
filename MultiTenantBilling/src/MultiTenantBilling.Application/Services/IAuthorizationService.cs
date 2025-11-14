using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiTenantBilling.Application.Services
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(string userEmail, string permission);
        Task<bool> HasRoleAsync(string userEmail, string roleName);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userEmail);
        Task<IEnumerable<string>> GetUserRolesAsync(string userEmail);
        Task<bool> AssignRoleAsync(string userEmail, string roleName);
    }
}