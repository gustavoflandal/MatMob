using Microsoft.EntityFrameworkCore;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Services;
using Xunit;

namespace AuditServiceTests;

public class BasicAuditTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public BasicAuditTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void AuditLog_Should_HaveRequiredProperties()
    {
        // Arrange & Act
        var auditLog = new AuditLog
        {
            Id = 1,
            Action = AuditActions.CREATE,
            EntityName = "TestEntity",
            EntityId = 123,
            Description = "Test audit log",
            UserId = "user123",
            CreatedAt = DateTime.UtcNow,
            Category = AuditCategory.DATA_MODIFICATION,
            Severity = AuditSeverity.INFO
        };

        // Assert
        Assert.Equal(1, auditLog.Id);
        Assert.Equal(AuditActions.CREATE, auditLog.Action);
        Assert.Equal("TestEntity", auditLog.EntityName);
        Assert.Equal(123, auditLog.EntityId);
        Assert.Equal("Test audit log", auditLog.Description);
        Assert.Equal("user123", auditLog.UserId);
        Assert.Equal(AuditCategory.DATA_MODIFICATION, auditLog.Category);
        Assert.Equal(AuditSeverity.INFO, auditLog.Severity);
    }

    [Fact]
    public async Task CanAddAuditLogToDatabase()
    {
        // Arrange
        var auditLog = new AuditLog
        {
            Action = AuditActions.CREATE,
            EntityName = "TestEntity",
            EntityId = 123,
            Description = "Test audit log",
            UserId = "user123",
            CreatedAt = DateTime.UtcNow,
            Category = AuditCategory.DATA_MODIFICATION,
            Severity = AuditSeverity.INFO
        };

        // Act
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        // Assert
        var savedLog = await _context.AuditLogs.FirstAsync();
        Assert.Equal(AuditActions.CREATE, savedLog.Action);
        Assert.Equal("TestEntity", savedLog.EntityName);
        Assert.Equal(123, savedLog.EntityId);
    }

    [Theory]
    [InlineData("CREATE", "CRUD", "INFO")]
    [InlineData("UPDATE", "CRUD", "INFO")]
    [InlineData("DELETE", "CRUD", "WARNING")]
    public void AuditLog_Should_AcceptValidCombinations(string action, string category, string severity)
    {
        // Arrange & Act
        var auditLog = new AuditLog
        {
            Action = action,
            Category = category,
            Severity = severity,
            EntityName = "TestEntity",
            Description = "Test description",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(action, auditLog.Action);
        Assert.Equal(category, auditLog.Category);
        Assert.Equal(severity, auditLog.Severity);
    }
}