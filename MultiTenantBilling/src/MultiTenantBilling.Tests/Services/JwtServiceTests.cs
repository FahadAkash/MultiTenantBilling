using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using MultiTenantBilling.Application.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace MultiTenantBilling.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private readonly JwtSettings _jwtSettings;

        public JwtServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                Secret = "THIS_IS_A_VERY_SECURE_KEY_THAT_SHOULD_BE_CHANGED_IN_PRODUCTION_1234567890",
                Issuer = "MultiTenantBilling",
                Audience = "MultiTenantBillingUsers",
                ExpiryInHours = 1
            };

            var optionsMock = new Mock<IOptions<JwtSettings>>();
            optionsMock.Setup(x => x.Value).Returns(_jwtSettings);

            _jwtService = new JwtService(optionsMock.Object);
        }

        [Fact]
        public void GenerateToken_WithValidUserDtoAndTenantId_ShouldGenerateValidToken()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                Roles = new[] { "User", "Admin" }
            };

            var tenantId = Guid.NewGuid();

            // Act
            var token = _jwtService.GenerateToken(userDto, tenantId);

            // Assert
            token.Should().NotBeNullOrEmpty();

            // Validate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            principal.Should().NotBeNull();
            validatedToken.Should().NotBeNull();

            // Check claims
            principal.HasClaim(ClaimTypes.NameIdentifier, userDto.Id.ToString()).Should().BeTrue();
            principal.HasClaim(ClaimTypes.Email, userDto.Email).Should().BeTrue();
            principal.HasClaim(ClaimTypes.GivenName, userDto.FirstName).Should().BeTrue();
            principal.HasClaim(ClaimTypes.Surname, userDto.LastName).Should().BeTrue();
            principal.HasClaim("tenantId", tenantId.ToString()).Should().BeTrue();
            
            // Check roles
            principal.HasClaim(ClaimTypes.Role, "User").Should().BeTrue();
            principal.HasClaim(ClaimTypes.Role, "Admin").Should().BeTrue();
        }

        [Fact]
        public void ValidateToken_WithValidToken_ShouldReturnPrincipal()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                Roles = new[] { "User" }
            };

            var tenantId = Guid.NewGuid();
            var token = _jwtService.GenerateToken(userDto, tenantId);

            // Act
            var principal = _jwtService.ValidateToken(token);

            // Assert
            principal.Should().NotBeNull();
            principal?.Identity?.IsAuthenticated.Should().BeTrue();
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var principal = _jwtService.ValidateToken(invalidToken);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_WithExpiredToken_ShouldReturnNull()
        {
            // Arrange - Create an expired token
            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                Roles = new[] { "User" }
            };

            var tenantId = Guid.NewGuid();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                    new Claim(ClaimTypes.Email, userDto.Email),
                    new Claim(ClaimTypes.GivenName, userDto.FirstName),
                    new Claim(ClaimTypes.Surname, userDto.LastName),
                    new Claim("tenantId", tenantId.ToString())
                }),
                // Set issued at to 2 hours ago and expires 1 hour ago to make it expired
                IssuedAt = DateTime.UtcNow.AddHours(-2),
                NotBefore = DateTime.UtcNow.AddHours(-2),
                Expires = DateTime.UtcNow.AddHours(-1), // Expired 1 hour ago
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var expiredToken = tokenHandler.WriteToken(token);

            // Act
            var principal = _jwtService.ValidateToken(expiredToken);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public void GenerateToken_WithUserWithoutRoles_ShouldGenerateValidToken()
        {
            // Arrange
            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                Roles = new string[] { } // No roles
            };

            var tenantId = Guid.NewGuid();

            // Act
            var token = _jwtService.GenerateToken(userDto, tenantId);

            // Assert
            token.Should().NotBeNullOrEmpty();

            // Validate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            principal.Should().NotBeNull();
            validatedToken.Should().NotBeNull();

            // Check claims
            principal.HasClaim(ClaimTypes.NameIdentifier, userDto.Id.ToString()).Should().BeTrue();
            principal.HasClaim(ClaimTypes.Email, userDto.Email).Should().BeTrue();
            principal.HasClaim(ClaimTypes.GivenName, userDto.FirstName).Should().BeTrue();
            principal.HasClaim(ClaimTypes.Surname, userDto.LastName).Should().BeTrue();
            principal.HasClaim("tenantId", tenantId.ToString()).Should().BeTrue();
            
            // Check that no role claims are present
            principal.HasClaim(ClaimTypes.Role, "User").Should().BeFalse();
        }
    }
}