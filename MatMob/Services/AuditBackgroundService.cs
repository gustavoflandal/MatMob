using MatMob.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace MatMob.Services;

public class AuditBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditBackgroundService> _logger;
    private readonly ConcurrentQueue<AuditLog> _auditLogsQueue;

    public AuditBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<AuditBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _auditLogsQueue = new ConcurrentQueue<AuditLog>();
    }

    public void EnqueueLog(AuditLog auditLog)
    {
        _auditLogsQueue.Enqueue(auditLog);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Normal quando o serviço está sendo parado
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processamento em background dos logs de auditoria");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Aguarda mais tempo em caso de erro
            }
        }

        // Processa qualquer log restante antes de parar
        await ProcessBatchAsync();
    }

    private async Task ProcessBatchAsync()
    {
        if (_auditLogsQueue.IsEmpty) return;

        var logsToSave = new List<AuditLog>();
        
        // Dequeue até 100 logs por vez
        while (_auditLogsQueue.TryDequeue(out var log) && logsToSave.Count < 100)
        {
            logsToSave.Add(log);
        }

        if (logsToSave.Any())
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                
                await dbContext.AuditLogs.AddRangeAsync(logsToSave);
                await dbContext.SaveChangesAsync();
                
                _logger.LogDebug("Processados {Count} logs de auditoria", logsToSave.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar {Count} logs de auditoria", logsToSave.Count);
                
                // Recoloca os logs na fila para tentar novamente
                foreach (var log in logsToSave)
                {
                    _auditLogsQueue.Enqueue(log);
                }
                
                throw;
            }
        }
    }
}