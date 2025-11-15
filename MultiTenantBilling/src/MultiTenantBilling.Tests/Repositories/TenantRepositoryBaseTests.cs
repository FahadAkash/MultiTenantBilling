using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MultiTenantBilling.Domain.Common;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Domain.Interface;
using MultiTenantBilling.Infrastructure.Data;
using MultiTenantBilling.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MultiTenantBilling.Tests.Repositories
{
    public class TenantRepositoryBaseTests
    {
        // Test entity class that implements ITenantEntity
        public class TestEntity : BaseEntity, ITenantEntity
        {
            public Guid TenantId { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        // Test repository class that works with TestEntity
        public class TestTenantRepository : TenantRepositoryBase<TestEntity>
        {
            public TestTenantRepository(ApplicationDbContext context) : base(context)
            {
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowNotImplementedException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase_GetByIdAsync")
                .Options;
            
            using var context = new ApplicationDbContext(options);
            var repository = new TestTenantRepository(context);
            var id = Guid.NewGuid();

            // Act
            Func<Task<TestEntity>> act = async () => await repository.GetByIdAsync(id);

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>()
                .WithMessage("Use GetByIdForTenantAsync instead or implement tenant filtering in concrete class");
        }

        [Fact]
        public async Task GetByIdForTenantAsync_WithExistingEntity_ShouldReturnEntity()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase_GetByIdForTenantAsync_Existing")
                .Options;
            
            using var context = new ApplicationDbContext(options);
            var repository = new TestTenantRepository(context);
            
            var tenantId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            
            // We'll use an existing entity type that's already registered
            var user = new User
            {
                Id = entityId,
                TenantId = tenantId,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "HASHED_password", // Add required PasswordHash
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Create a repository for User instead
            var userRepository = new UserRepository(context);
            
            // Act
            var result = await userRepository.GetByIdForTenantAsync(entityId, tenantId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(entityId);
            result.TenantId.Should().Be(tenantId);
            result.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetByIdForTenantAsync_WithNonExistentEntity_ShouldReturnNull()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase_GetByIdForTenantAsync_NonExistent")
                .Options;
            
            using var context = new ApplicationDbContext(options);
            var repository = new UserRepository(context); // Use UserRepository for testing
            
            var tenantId = Guid.NewGuid();
            var entityId = Guid.NewGuid();

            // Act
            var result = await repository.GetByIdForTenantAsync(entityId, tenantId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByTenantIdAsync_WithMultipleEntities_ShouldReturnFilteredEntities()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase_GetByTenantIdAsync")
                .Options;
            
            using var context = new ApplicationDbContext(options);
            var repository = new UserRepository(context); // Use UserRepository for testing
            
            var tenantId1 = Guid.NewGuid();
            var tenantId2 = Guid.NewGuid();
            
            var users = new[]
            {
                new User { Id = Guid.NewGuid(), TenantId = tenantId1, Email = "user1@example.com", FirstName = "User", LastName = "One", PasswordHash = "HASHED_password1", IsActive = true },
                new User { Id = Guid.NewGuid(), TenantId = tenantId1, Email = "user2@example.com", FirstName = "User", LastName = "Two", PasswordHash = "HASHED_password2", IsActive = true },
                new User { Id = Guid.NewGuid(), TenantId = tenantId2, Email = "user3@example.com", FirstName = "User", LastName = "Three", PasswordHash = "HASHED_password3", IsActive = true }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByTenantIdAsync(tenantId1);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.All(e => e.TenantId == tenantId1).Should().BeTrue();
        }
    }
}