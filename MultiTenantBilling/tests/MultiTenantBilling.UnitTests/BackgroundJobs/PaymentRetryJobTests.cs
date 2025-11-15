using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MultiTenantBilling.Application.BackgroundJobs;
using MultiTenantBilling.Application.Commands;
using MultiTenantBilling.Application.Queries;
using MultiTenantBilling.Application.DTOs;

namespace MultiTenantBilling.UnitTests.BackgroundJobs
{
    public class PaymentRetryJobTests
    {
        [Fact]
        public async Task ExecuteAsync_WithFailedPayments_ShouldRetryPayments()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<PaymentRetryJob>>();
            
            var failedPayments = new List<FailedPaymentDto>
            {
                new FailedPaymentDto
                {
                    InvoiceId = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    RetryAttempt = 1,
                    LastAttemptDate = DateTime.UtcNow.AddDays(-1),
                    Amount = 100.0m,
                    FailureReason = "Payment failed"
                }
            };
            
            mediatorMock.Setup(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedPayments);
                
            mediatorMock.Setup(m => m.Send(It.IsAny<RetryFailedPaymentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            var job = new PaymentRetryJob(mediatorMock.Object, loggerMock.Object);
            
            // Act
            await job.ExecuteAsync();
            
            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.Verify(m => m.Send(It.IsAny<RetryFailedPaymentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting payment retry job")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ExecuteAsync_WhenRetryFails_ShouldLogWarningAndContinue()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<PaymentRetryJob>>();
            
            var failedPayments = new List<FailedPaymentDto>
            {
                new FailedPaymentDto
                {
                    InvoiceId = Guid.NewGuid(),
                    TenantId = Guid.NewGuid(),
                    RetryAttempt = 1,
                    LastAttemptDate = DateTime.UtcNow.AddDays(-1),
                    Amount = 100.0m,
                    FailureReason = "Payment failed"
                }
            };
            
            mediatorMock.Setup(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedPayments);
                
            mediatorMock.Setup(m => m.Send(It.IsAny<RetryFailedPaymentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // Retry failed
            
            var job = new PaymentRetryJob(mediatorMock.Object, loggerMock.Object);
            
            // Act
            await job.ExecuteAsync();
            
            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            mediatorMock.Verify(m => m.Send(It.IsAny<RetryFailedPaymentCommand>(), It.IsAny<CancellationToken>()), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Payment retry failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ExecuteAsync_WhenQueryThrowsException_ShouldLogError()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var loggerMock = new Mock<ILogger<PaymentRetryJob>>();
            
            mediatorMock.Setup(m => m.Send(It.IsAny<GetFailedPaymentsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));
            
            var job = new PaymentRetryJob(mediatorMock.Object, loggerMock.Object);
            
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => job.ExecuteAsync());
            
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in payment retry job")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}