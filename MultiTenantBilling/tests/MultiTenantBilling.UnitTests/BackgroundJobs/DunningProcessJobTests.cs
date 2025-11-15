using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MultiTenantBilling.Application.BackgroundJobs;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Domain.Entities;
using MultiTenantBilling.Infrastructure.Repositories;
using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.UnitTests.BackgroundJobs
{
    public class DunningProcessJobTests
    {
        [Fact]
        public async Task ExecuteAsync_WithFailedPayments_ShouldProcessDunning()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var invoiceRepositoryMock = new Mock<ITenantRepository<Invoice>>();
            var subscriptionRepositoryMock = new Mock<ITenantRepository<Subscription>>();
            var loggerMock = new Mock<ILogger<DunningProcessJob>>();
            
            var failedPayments = new List<FailedPaymentDto>
            {
                new FailedPaymentDto
                {
                    InvoiceId = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    RetryAttempt = 1,
                    LastAttemptDate = DateTime.UtcNow.AddDays(-1)
                }
            };
            
            var invoice = new Invoice
            {
                Id = failedPayments[0].InvoiceId,
                TenantId = failedPayments[0].TenantId,
                Amount = 100.0m,
                Status = "Overdue"
            };
            
            mediatorMock.Setup(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedPayments);
                
            invoiceRepositoryMock.Setup(r => r.GetByIdAsync(failedPayments[0].InvoiceId))
                .ReturnsAsync(invoice);
            
            var job = new DunningProcessJob(
                mediatorMock.Object, 
                invoiceRepositoryMock.Object, 
                subscriptionRepositoryMock.Object, 
                loggerMock.Object);
            
            // Act
            await job.ExecuteAsync();
            
            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            invoiceRepositoryMock.Verify(r => r.GetByIdAsync(failedPayments[0].InvoiceId), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting dunning process job")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ExecuteAsync_WhenPaymentIsOld_ShouldSuspendSubscription()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var invoiceRepositoryMock = new Mock<ITenantRepository<Invoice>>();
            var subscriptionRepositoryMock = new Mock<ITenantRepository<Subscription>>();
            var loggerMock = new Mock<ILogger<DunningProcessJob>>();
            
            var failedPayments = new List<FailedPaymentDto>
            {
                new FailedPaymentDto
                {
                    InvoiceId = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    RetryAttempt = 1,
                    LastAttemptDate = DateTime.UtcNow.AddDays(-14) // 14 days old
                }
            };
            
            var invoice = new Invoice
            {
                Id = failedPayments[0].InvoiceId,
                TenantId = failedPayments[0].TenantId,
                Amount = 100.0m,
                Status = "Overdue"
            };
            
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    TenantId = failedPayments[0].TenantId,
                    Status = "Active"
                }
            };
            
            mediatorMock.Setup(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedPayments);
                
            invoiceRepositoryMock.Setup(r => r.GetByIdAsync(failedPayments[0].InvoiceId))
                .ReturnsAsync(invoice);
                
            subscriptionRepositoryMock.Setup(r => r.GetByTenantIdAsync(failedPayments[0].TenantId))
                .ReturnsAsync(subscriptions);
            
            var job = new DunningProcessJob(
                mediatorMock.Object, 
                invoiceRepositoryMock.Object, 
                subscriptionRepositoryMock.Object, 
                loggerMock.Object);
            
            // Act
            await job.ExecuteAsync();
            
            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.Verify(m => m.Send(It.IsAny<SuspendSubscriptionCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Suspending subscription for tenant")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ExecuteAsync_WhenQueryThrowsException_ShouldLogError()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var invoiceRepositoryMock = new Mock<ITenantRepository<Invoice>>();
            var subscriptionRepositoryMock = new Mock<ITenantRepository<Subscription>>();
            var loggerMock = new Mock<ILogger<DunningProcessJob>>();
            
            mediatorMock.Setup(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));
            
            var job = new DunningProcessJob(
                mediatorMock.Object, 
                invoiceRepositoryMock.Object, 
                subscriptionRepositoryMock.Object, 
                loggerMock.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => job.ExecuteAsync());
            
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in dunning process job")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}