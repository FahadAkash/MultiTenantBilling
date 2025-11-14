using MultiTenantBilling.Application.Services;
using System;

namespace MultiTenantBilling.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = default!;
    }
}