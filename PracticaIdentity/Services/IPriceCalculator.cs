
using System.Threading;
using System.Threading.Tasks;
using CapaData.Data;

namespace PriceRecalculationMvc_VS2022.Services;

public interface IPriceCalculator
{
    Task<PriceCalcResult> RecalculateAsync(DB_Context db, CancellationToken ct);
}

public record PriceCalcResult(int ProductsUpdated, int PromotionsApplied);
