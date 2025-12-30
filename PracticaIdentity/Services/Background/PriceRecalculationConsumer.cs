
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CapaData.Data;
using CapaData.Models;
using PriceRecalculationMvc_VS2022.Services;

namespace PriceRecalculationMvc_VS2022.Services.Background;

public class PriceRecalculationConsumer : BackgroundService
{
    private readonly ILogger<PriceRecalculationConsumer> _logger;
    private readonly IJobsQueue _queue;
    private readonly IJobsStore _store;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _maxConcurrency;

    public PriceRecalculationConsumer(
        ILogger<PriceRecalculationConsumer> logger,
        IJobsQueue queue,
        IJobsStore store,
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _logger = logger;
        _queue = queue;
        _store = store;
        _scopeFactory = scopeFactory;
        _maxConcurrency = config.GetValue<int?>("Jobs:MaxConcurrency") ?? 1;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var semaphore = new SemaphoreSlim(_maxConcurrency);
        await foreach (var job in _queue.ReadAllAsync(stoppingToken))
        {
            await semaphore.WaitAsync(stoppingToken);
            _ = ProcessJobAsync(job, semaphore, stoppingToken);
        }
    }

    private async Task ProcessJobAsync(PriceJob job, SemaphoreSlim semaphore, CancellationToken ct)
    {
        try
        {
            _store.ChangeState(job.Id, JobState.Processing);
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DB_Context>();
            var calculator = scope.ServiceProvider.GetRequiredService<IPriceCalculator>();

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            var result = await ExecuteWithRetriesAsync(() => calculator.RecalculateAsync(db, ct), ct, 3);
            await tx.CommitAsync(ct);

            _store.ChangeState(job.Id, JobState.Completed);
            _logger.LogInformation("Job {Id} completado. Productos: {Updated}, Promos: {Promos}", job.Id, result.ProductsUpdated, result.PromotionsApplied);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("Job {Id} cancelado por apagado.", job.Id);
            _store.ChangeState(job.Id, JobState.Error, "Cancelado por apagado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando job {Id}", job.Id);
            _store.ChangeState(job.Id, JobState.Error, ex.Message);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static async Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> action, CancellationToken ct, int maxAttempts)
    {
        int attempt = 0; var delay = TimeSpan.FromSeconds(1);
        while (true)
        {
            try { return await action(); }
            catch (Exception) when (attempt < maxAttempts)
            { attempt++; await Task.Delay(delay, ct); delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30)); }
        }
    }
}
