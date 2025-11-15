using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MultiTenantBilling.Application.BackgroundJobs;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.UnitTests.BackgroundJobs
{
    public class InvoiceGenerationJobTests
    {
        [Fact]
        public async Task ExecuteAsync_WithValidData_ShouldGenerateInvoices()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<InvoiceGenerationJob>>();
            
            var upcomingBilling = new List<UpcomingBillingDto>
            {
                new UpcomingBillingDto
                {
                    SubscriptionId = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    PlanId = Guid.NewGuid(),
                    NextBillingDate = DateTime.UtcNow,
                    TenantName = "Test Tenant"
                }
            };
            
            var generatedInvoice = new InvoiceDto
            {
                Id = Guid.NewGuid(),
                TenantId = upcomingBilling[0].TenantId,
                SubscriptionId = upcomingBilling[0].SubscriptionId,
                Amount = 100.0m,
                Status = "Pending"
            };
            
            mediatorMock.Setup(m => m.Send(It.IsAny<GetUpcomingBillingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(upcomingBilling);
                
            mediatorMock.Setup(m => m.Send(It.IsAny<GenerateInvoiceCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(generatedInvoice);
            
            var job = new InvoiceGenerationJob(mediatorMock.Object, loggerMock.Object);
            
            // Act
            await job.ExecuteAsync();
            
            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<GetUpcomingBillingQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.Verify(m => m.Send(It.IsAny<GenerateInvoiceCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting invoice generation job")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ExecuteAsync_WhenQueryThrowsException_ShouldLogError()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<InvoiceGenerationJob>>();
            
            mediatorMock.Setup(m => m.Send(It.IsAny<GetUpcomingBillingQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));
            
            var job = new InvoiceGenerationJob(mediatorMock.Object, loggerMock.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => job.ExecuteAsync());
            
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in invoice generation job")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}