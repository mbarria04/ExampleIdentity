using CapaData.Data;
using CapaData.Models;
using PriceRecalculationMvc_VS2022.Services;

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

    private async Task ProcessJobAsync(
    PriceJob job,
    SemaphoreSlim semaphore,
    CancellationToken ct)
    {
        try
        {
            _store.ChangeState(job.Id, JobState.Processing);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DB_Context>();
            var calculator = scope.ServiceProvider.GetRequiredService<IPriceCalculator>();



            var result = await calculator.RecalculateAsync(db, job.Id, job.StoredProc, ct);

            _store.ChangeState(job.Id, JobState.Completed);

            _logger.LogInformation(
                "Job {Id} completado. Productos: {Updated}, Promos: {Promos}",
                job.Id,
                result.ProductsUpdated,
                result.PromotionsApplied
            );
        }
        catch (OperationCanceledException)
        {
            _store.ChangeState(job.Id, JobState.Error, "Cancelado");
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

}
