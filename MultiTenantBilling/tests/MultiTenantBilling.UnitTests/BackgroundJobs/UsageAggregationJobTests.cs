using Microsoft.Extensions.Logging;
using Moq;
using MultiTenantBilling.Application.BackgroundJobs;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;

namespace MultiTenantBilling.UnitTests.BackgroundJobs
{
    public class UsageAggregationJobTests
    {
        [Fact]
        public async Task ExecuteAsync_WithUsageRecords_ShouldAggregateUsage()
        {
            // Arrange
            var usageRepositoryMock = new Mock<ITenantRepository<UsageRecord>>();
            var loggerMock = new Mock<ILogger<UsageAggregationJob>>();
            
            var usageRecords = new List<UsageRecord>
            {
                new UsageRecord
                {
                    Id = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    SubscriptionId = Guid.NewGuid(),
                    MetricName = "api_calls",
                    Quantity = 100,
                    RecordedAt = DateTime.UtcNow.AddMinutes(-30)
                },
                new UsageRecord
                {
                    Id = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    SubscriptionId = Guid.NewGuid(),
                    MetricName = "api_calls",
                    Quantity = 150,
                    RecordedAt = DateTime.UtcNow.AddMinutes(-15)
                }
            };
            
            usageRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(usageRecords);
            
            var job = new UsageAggregationJob(usageRepositoryMock.Object, loggerMock.Object);
            
            // Act
            await job.ExecuteAsync();
            
            // Assert
            usageRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting usage aggregation job")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldLogError()
        {
            // Arrange
            var usageRepositoryMock = new Mock<ITenantRepository<UsageRecord>>();
            var loggerMock = new Mock<ILogger<UsageAggregationJob>>();
            
            usageRepositoryMock.Setup(r => r.GetAllAsync())
                .ThrowsAsync(new Exception("Database error"));
            
            var job = new UsageAggregationJob(usageRepositoryMock.Object, loggerMock.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => job.ExecuteAsync());
            
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in usage aggregation job")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}