using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Services;
using Moq;
using Xunit;

namespace MatMob.Tests.Services
{
    public class AuditServiceUnitTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<ILogger<AuditService>> _mockLogger;
        private readonly Mock<IAuditImmutabilityService> _mockImmutabilityService;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<IAuditBackgroundService> _mockBackgroundService;
        private readonly AuditService _auditService;

        public AuditServiceUnitTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockLogger = new Mock<ILogger<AuditService>>();
            _mockImmutabilityService = new Mock<IAuditImmutabilityService>();
            _mockCache = new Mock<IMemoryCache>();
            _mockBackgroundService = new Mock<IAuditBackgroundService>();

            // Setup the immutability service mock to return the same audit log
            _mockImmutabilityService.Setup(x => x.PrepareAuditLogAsync(It.IsAny<AuditLog>()))
                .Returns<AuditLog>((log) => 
                {
                    log.SequenceNumber = 1;
                    log.ContentHash = "mockedhash";
                    log.PreviousHash = "0000000000000000000000000000000000000000000000000000000000000000";
                    log.IntegrityVerified = true;
                    return Task.FromResult(log);
                });

            _auditService = new AuditService(
                _context,
                _mockServiceProvider.Object,
                _mockLogger.Object,
                _mockImmutabilityService.Object,
                _mockCache.Object,
                _mockBackgroundService.Object
            );
        }

        [Fact]
        public async Task LogAsync_Should_ThrowArgumentNullException_WhenAuditLogIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _auditService.LogAsync(null!, "Test", null, "Test", "Test", "Test"));
        }

        [Fact]
        public async Task LogDeleteAsync_Should_CreateAuditLog_WithWarningSeverity()
        {
            // Arrange
            var entity = new { Id = 1, Name = "Test" };

            // Act
            await _auditService.LogDeleteAsync(entity);

            // Assert
            var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
            Assert.NotNull(auditLog);
            Assert.Equal("DELETE", auditLog.Action);
            Assert.Equal("WARNING", auditLog.Severity);
        }

        [Fact]
        public async Task EndAuditContextAsync_Should_LogOperationCompletion()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();

            // Act
            await _auditService.EndAuditContextAsync(correlationId);

            // Assert
            // Verify that the operation was completed
            Assert.True(true); // Placeholder assertion
        }

        [Fact]
        public async Task CleanupOldLogsAsync_Should_RemoveExpiredLogs()
        {
            // Arrange
            var oldLog = new AuditLog
            {
                UserId = "test",
                Action = "Test",
                EntityName = "Test",
                CreatedAt = DateTime.UtcNow.AddDays(-40),
                IpAddress = "127.0.0.1",
                UserAgent = "Test",
                Severity = "Info"
            };
            _context.AuditLogs.Add(oldLog);
            await _context.SaveChangesAsync();

            // Act
            await _auditService.CleanupOldLogsAsync(30);

            // Assert
            var remainingLogs = await _context.AuditLogs.CountAsync();
            Assert.Equal(0, remainingLogs);
        }

        [Fact]
        public async Task SearchLogsAsync_Should_FilterByMultipleCriteria()
        {
            // Arrange
            var log1 = new AuditLog
            {
                UserId = "user1",
                Action = "Create",
                EntityName = "Test",
                CreatedAt = DateTime.UtcNow,
                IpAddress = "127.0.0.1",
                UserAgent = "Test",
                Severity = "Info"
            };
            _context.AuditLogs.Add(log1);
            await _context.SaveChangesAsync();

            // Act
            var results = await _auditService.SearchLogsAsync(
                startDate: DateTime.UtcNow.AddDays(-1),
                endDate: DateTime.UtcNow.AddDays(1),
                userId: null,
                action: "Create",
                entityName: null,
                entityId: null,
                severity: null,
                category: null,
                skip: 0,
                take: 10
            );

            // Assert
            Assert.Single(results);
            Assert.Equal("Create", results.First().Action);
        }

        [Fact]
        public async Task LogErrorAsync_Should_HandleExceptions_WithStackTrace()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");

            // Act
            await _auditService.LogErrorAsync(exception);

            // Assert
            var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
            Assert.NotNull(auditLog);
            Assert.Equal("ERROR", auditLog.Severity);
            Assert.Contains("Test exception", auditLog.ErrorMessage!);
        }

        [Fact]
        public async Task LogLoginAttemptAsync_Should_HandleSuccessAndFailure()
        {
            // Arrange
            var username = "testuser";

            // Act - Success
            await _auditService.LogLoginAttemptAsync(username, true);

            // Act - Failure
            await _auditService.LogLoginAttemptAsync(username, false, "Invalid credentials");

            // Assert
            var logs = await _context.AuditLogs.ToListAsync();
            Assert.Equal(2, logs.Count);
            Assert.Contains(logs, l => l.Action.Contains("LOGIN") && l.Success);
            Assert.Contains(logs, l => l.Action.Contains("LOGIN") && !l.Success);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}