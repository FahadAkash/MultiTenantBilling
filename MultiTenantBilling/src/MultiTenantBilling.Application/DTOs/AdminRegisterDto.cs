using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Application.DTOs
{
    public class AdminRegisterDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public Guid TenantId { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}