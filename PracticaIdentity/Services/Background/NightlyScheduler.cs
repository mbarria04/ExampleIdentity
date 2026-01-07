
using CapaAplicacion;
using CapaData.Data;
using CapaData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PriceRecalculationMvc_VS2022.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PriceRecalculationMvc_VS2022.Services.Background
{
    public class NightlyScheduler : BackgroundService
    {
        private readonly ILogger<NightlyScheduler> _logger;
        private readonly IConfiguration _config;
        private readonly IJobsStore _store;
        private readonly IDbContextFactory<DB_Context> _dbFactory;
        private readonly IPriceCalculator _calculator;
        private readonly Dependencias _dependencias;
        public NightlyScheduler(
            ILogger<NightlyScheduler> logger,
            IConfiguration config,
            IJobsStore store,
            IDbContextFactory<DB_Context> dbFactory,
            IPriceCalculator calculator, Dependencias dependencias)
        {
            _logger = logger;
            _config = config;
            _store = store;
            _dbFactory = dbFactory;
            _calculator = calculator;
            _dependencias = dependencias;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var runTimeStr = _config["Scheduler:RunTime"] ?? "11:00:00";
            if (!TimeSpan.TryParse(runTimeStr, out var runTime))
                runTime = TimeSpan.FromHours(2);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                var nextRun = NextRun(now, runTime);
                _logger.LogInformation("Próxima ejecución diaria programada para {NextRun}", nextRun);

                try
                {
                    // Espera hasta la hora programada
                    var delay = nextRun - now;
                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                // En este punto: es hora de ejecutar
                await RunNightlyAsync(stoppingToken);
            }
        }

        private static DateTimeOffset NextRun(DateTimeOffset now, TimeSpan runTime)
        {
            var todayRun = new DateTimeOffset(
                now.Year, now.Month, now.Day,
                runTime.Hours, runTime.Minutes, runTime.Seconds,
                now.Offset);

            return now <= todayRun ? todayRun : todayRun.AddDays(1);
        }

        /// <summary>
        /// Ejecuta el ciclo nightly: procesa jobs pendientes y (opcional) crea/ejecuta el nightly.
        /// </summary>
        private async Task RunNightlyAsync(CancellationToken ct)
        {
            _logger.LogInformation("NightlyScheduler disparado a las {Now}", DateTimeOffset.Now);

            // 1) Procesar TODOS los 'Pending' existentes (insertados manualmente o antes)
            await ProcessPendingJobsAsync(ct);

            // 2) (Opcional) Crear un job "Nightly" y procesarlo inmediatamente (sin encolar)
            var nightlyJob = new PriceJob(Guid.NewGuid(), DateTimeOffset.UtcNow, "Nightly");
            _store.Insert(nightlyJob);
            await ProcessSingleJobAsync(nightlyJob, ct);

            _logger.LogInformation("NightlyScheduler terminó ciclo.");
        }

        /// <summary>
        /// Busca y procesa en BD los jobs con State='Pending'.
        /// </summary>
        private async Task ProcessPendingJobsAsync(CancellationToken ct)
        {
            using var db = _dbFactory.CreateDbContext();

            // Trae un lote de PENDING (ajusta el Take si esperas muchos)
            var pending = await db.Jobs
                .Where(j => j.State == "Pending")
                .OrderBy(j => j.Created)
                .Take(500)
                .AsNoTracking()
                .ToListAsync(ct);

            foreach (var j in pending)
            {
                // Reclamar atómicamente el job

                var rows = await db.Database.ExecuteSqlInterpolatedAsync($@"
               UPDATE dbo.Jobs
                 SET State = {"Processing"}
                   WHERE Id = {j.Id} AND State = {"Pending"}", cancellationToken: ct);


                if (rows == 1)
                {
                    // Ejecutar cálculo directamente (sin cola)
                    var job = new PriceJob(j.Id, j.Created, j.Reason);
                    await ProcessSingleJobAsync(job, ct);
                }
                else
                {
                    _logger.LogInformation("Job {Id} ya fue reclamado/procesado por otro proceso.", j.Id);
                }
            }
        }

        /// <summary>
        /// Ejecuta el cálculo de precios para un Job y actualiza estado Completed/Error.
        /// </summary>
        /// 

        private async Task ProcessSingleJobAsync(
    PriceJob job,
    CancellationToken ct)
        {
            try
            {
                using var db = _dbFactory.CreateDbContext();

                var calcResult = await _calculator.RecalculateAsync(
                    db,
                    job.Id,
                    job.StoredProc,
                    ct);

                var update = await _dependencias.PromocionServices
                    .MarcarJobCompletadoAsync(job.Id, ct);

                if (!update.Success)
                {
                    _logger.LogWarning(
                        "Job {JobId} ejecutado pero no se pudo marcar como completado",
                        job.Id);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                await _dependencias.PromocionServices.MarcarJobFallidoAsync(
                    job.Id,
                    "Cancelado por detención del servicio",
                    ct);
            }
            catch (Exception ex)
            {
                await _dependencias.PromocionServices.MarcarJobFallidoAsync(
                    job.Id,
                    ex.Message,
                    ct);
            }


            //private async Task ProcessSingleJobAsync(PriceJob job, CancellationToken ct)
            //{
            //    try
            //    {
            //        using var db = _dbFactory.CreateDbContext();

            //        // Ejecuta SP con JobId y StoredProc guardado en Jobs
            //        var result = await _calculator.RecalculateAsync(db, job.Id, job.StoredProc, ct);

            //        // Estado final en BD: Completed y Error NULL
            //        await db.Database.ExecuteSqlInterpolatedAsync($@"
            //    UPDATE dbo.Jobs
            //       SET State = {"Completed"}, Error = {(string?)null}
            //     WHERE Id = {job.Id} AND State = {"Processing"}", ct);

            //        _logger.LogInformation(
            //            "Job {Id} COMPLETADO. Productos: {Updated}, Promos: {Promos}",
            //            job.Id, result.ProductsUpdated, result.PromotionsApplied);
            //    }
            //    catch (OperationCanceledException) when (ct.IsCancellationRequested)
            //    {
            //        using var db = _dbFactory.CreateDbContext();
            //        await db.Database.ExecuteSqlInterpolatedAsync($@"
            //    UPDATE dbo.Jobs
            //       SET State = {"Failed"}, Error = {"Cancelado por detención"}
            //     WHERE Id = {job.Id}", ct);

            //        _logger.LogWarning("Job {Id} cancelado por detención.", job.Id);
            //    }
            //    catch (Exception ex)
            //    {
            //        using var db = _dbFactory.CreateDbContext();
            //        await db.Database.ExecuteSqlInterpolatedAsync($@"
            //    UPDATE dbo.Jobs
            //       SET State = {"Failed"}, Error = {ex.Message}
            //     WHERE Id = {job.Id}", ct);

            //        _logger.LogError(ex, "Job {Id} FALLÓ durante ejecución de SP {Proc}.", job.Id, job.StoredProc);
            //    }
            //}

        }
    }
}
