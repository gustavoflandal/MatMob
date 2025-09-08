using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using MatMob.Data;
using MatMob.Models.Entities;
using MatMob.Services;
using Xunit;
using Xunit.Abstractions;

namespace MatMob.Tests.Services;

public class AuditServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public AuditServiceIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task AuditService_Should_CreateLog_WhenCalled()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        await auditService.LogAsync(new AuditLog
        {
            Action = "TestAction",
            Category = "TEST",
            Description = "Teste de integração",
            EntityName = "TestEntity",
            Severity = "INFO",
            Success = true
        });

        // Wait for background processing
        await Task.Delay(2000);

        // Assert
        var logs = await context.AuditLogs
            .Where(l => l.Action == "TestAction")
            .ToListAsync();

        Assert.NotEmpty(logs);
        var log = logs.First();
        Assert.Equal("TEST", log.Category);
        Assert.Equal("Teste de integração", log.Description);
        Assert.True(log.Success);
    }

    [Fact]
    public async Task AuditService_Should_ProcessBatch_WhenMultipleLogsAdded()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear any audit configurations to enable all logs
        var existingConfigs = await context.AuditModuleConfigs.ToListAsync();
        context.AuditModuleConfigs.RemoveRange(existingConfigs);
        await context.SaveChangesAsync();

        var initialCount = await context.AuditLogs.CountAsync();

        // Act
        for (int i = 0; i < 5; i++)
        {
            await auditService.LogAsync($"BatchTest", 
                description: $"Log batch #{i}",
                category: "BATCH_TEST");
        }

        // Force processing by saving changes
        await context.SaveChangesAsync();

        // Debug: Count all logs after batch processing
        var allLogsAfter = await context.AuditLogs.ToListAsync();
        var batchLogsInMemory = allLogsAfter.Where(l => l.Action == "BatchTest").ToList();

        // Assert
        var finalCount = await context.AuditLogs.CountAsync();
        Assert.True(finalCount >= initialCount + 5, $"Expected at least {initialCount + 5} logs, but found {finalCount}. InitialCount: {initialCount}, BatchLogs found: {batchLogsInMemory.Count}");

        var batchLogs = await context.AuditLogs
            .Where(l => l.Action == "BatchTest")
            .ToListAsync();

        Assert.True(batchLogs.Count >= 5, $"Expected at least 5 batch logs, but found {batchLogs.Count}");
    }

    [Fact]
    public async Task AuditService_Should_CreateImmutableHashes_WhenLogging()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        await auditService.LogAsync(new AuditLog
        {
            Action = "HashTest",
            Category = "HASH_TEST",
            Description = "Teste de hash imutável",
            EntityName = "HashEntity",
            Severity = "INFO",
            Success = true
        });

        // Wait for background processing
        await Task.Delay(2000);

        // Assert
        var log = await context.AuditLogs
            .FirstOrDefaultAsync(l => l.Action == "HashTest");

        Assert.NotNull(log);
        Assert.NotNull(log.ContentHash);
        Assert.NotEmpty(log.ContentHash);
        Assert.True(log.SequenceNumber > 0);
        Assert.True(log.IntegrityVerified);
    }

    [Fact]
    public async Task BackgroundService_Should_ProcessLogs_Continuously()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var backgroundService = scope.ServiceProvider.GetRequiredService<IAuditBackgroundService>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear any audit configurations to enable all logs
        var existingConfigs = await context.AuditModuleConfigs.ToListAsync();
        context.AuditModuleConfigs.RemoveRange(existingConfigs);
        await context.SaveChangesAsync();

        var initialCount = await context.AuditLogs.CountAsync();

        // Act
        await auditService.LogAsync("BackgroundTest",
            description: "Teste do serviço em background",
            category: "BACKGROUND_TEST");

        // Force processing by saving changes
        await context.SaveChangesAsync();

        // Assert
        var finalCount = await context.AuditLogs.CountAsync();
        Assert.True(finalCount > initialCount, $"Expected more than {initialCount} logs, but found {finalCount}");

        var backgroundLog = await context.AuditLogs
            .FirstOrDefaultAsync(l => l.Action == "BackgroundTest");

        Assert.NotNull(backgroundLog);
        Assert.Equal("BACKGROUND_TEST", backgroundLog.Category);
    }
}

