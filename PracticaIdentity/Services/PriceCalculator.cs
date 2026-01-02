
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CapaData.Data;
using Microsoft.Data.SqlClient;          // Ojo: Microsoft.Data.SqlClient
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PriceRecalculationMvc_VS2022.Services;

public class PriceCalculator : IPriceCalculator
{
    private readonly ILogger<PriceCalculator> _logger;

    // Lista blanca de SPs permitidos
    private static readonly HashSet<string> AllowedStoredProcs = new(StringComparer.OrdinalIgnoreCase)
    {
        "dbo.sp_RecalculatePricesForValidPromotions"
        // Agrega aquí otros nombres si luego amplías funcionalidades
    };

    public PriceCalculator(ILogger<PriceCalculator> logger)
    {
        _logger = logger;
    }

    public async Task<PriceCalcResult> RecalculateAsync(DB_Context db, Guid jobId, string storedProcName, CancellationToken ct)
    {
        if (!AllowedStoredProcs.Contains(storedProcName))
        {
            _logger.LogWarning("Intento de ejecutar SP no permitido: {Proc}", storedProcName);
            throw new InvalidOperationException($"StoredProc no permitido: {storedProcName}");
        }

        var pJobId = new SqlParameter("@JobId", SqlDbType.UniqueIdentifier) { Value = jobId };
        var pUpdated = new SqlParameter("@ProductsUpdated", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var pApplied = new SqlParameter("@PromotionsApplied", SqlDbType.Int) { Direction = ParameterDirection.Output };

        var sql = $"EXEC {storedProcName} @JobId = @JobId, @ProductsUpdated = @ProductsUpdated OUTPUT, @PromotionsApplied = @PromotionsApplied OUTPUT";

        try
        {
            await db.Database.ExecuteSqlRawAsync(sql, new[] { pJobId, pUpdated, pApplied }, ct);

            var productsUpdated = (pUpdated.Value is int u) ? u : 0;
            var promotionsApplied = (pApplied.Value is int a) ? a : 0;

            _logger.LogInformation(
                "SP {Proc} ejecutado para Job {JobId}. ProductsUpdated={Updated}, PromotionsApplied={Applied}",
                storedProcName, jobId, productsUpdated, promotionsApplied);

            return new PriceCalcResult(productsUpdated, promotionsApplied);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Ejecución cancelada para SP {Proc}, Job {JobId}", storedProcName, jobId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo ejecutando SP {Proc} para Job {JobId}", storedProcName, jobId);
            throw;
        }
    }
}
