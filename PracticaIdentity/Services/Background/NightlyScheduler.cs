
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CapaData.Models;
using PriceRecalculationMvc_VS2022.Services;

namespace PriceRecalculationMvc_VS2022.Services.Background;

public class NightlyScheduler : BackgroundService
{
    private readonly ILogger<NightlyScheduler> _logger;
    private readonly IConfiguration _config;
    private readonly IJobsQueue _queue;
    private readonly IJobsStore _store;

    public NightlyScheduler(ILogger<NightlyScheduler> logger, IConfiguration config, IJobsQueue queue, IJobsStore store)
    { _logger = logger; _config = config; _queue = queue; _store = store; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var runTimeStr = _config["Scheduler:RunTime"] ?? "16:26:00";
        if (!TimeSpan.TryParse(runTimeStr, out var runTime)) runTime = TimeSpan.FromHours(2);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.Now;
            var nextRun = NextRun(now, runTime);
            _logger.LogInformation("Próxima ejecución diaria programada para {NextRun}", nextRun);

            try { await Task.Delay(nextRun - now, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }

            var job = new PriceJob(Guid.NewGuid(), DateTimeOffset.Now, "Nightly");
            _store.Insert(job);
            await _queue.EnqueueAsync(job, stoppingToken);
            _logger.LogInformation("Job nocturno encolado: {Id}", job.Id);
        }
    }

    private static DateTimeOffset NextRun(DateTimeOffset now, TimeSpan runTime)
    {
        var todayRun = new DateTimeOffset(now.Year, now.Month, now.Day, runTime.Hours, runTime.Minutes, runTime.Seconds, now.Offset);
        return now <= todayRun ? todayRun : todayRun.AddDays(1);
    }
}
