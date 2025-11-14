using System;
using System.Collections.Generic;

namespace MultiTenantBilling.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public bool IsActive { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}