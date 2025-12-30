
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CapaData.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace PriceRecalculationMvc_VS2022.Services;

public class PriceCalculator : IPriceCalculator
{
    private readonly ILogger<PriceCalculator> _logger;
    public PriceCalculator(ILogger<PriceCalculator> logger) => _logger = logger;

    public async Task<PriceCalcResult> RecalculateAsync(DB_Context db, CancellationToken ct)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var activePromos = await db.Promotions
            .Where(p => p.StartUtc <= nowUtc && p.EndUtc >= nowUtc)
            .ToListAsync(ct);

        int promosApplied = 0;
        int productsUpdated = 0;

        var skus = activePromos.Select(p => p.ProductSku).Distinct().ToList();
        var products = await db.Products.Where(x => skus.Contains(x.Sku)).ToListAsync(ct);

        foreach (var product in products)
        {
            ct.ThrowIfCancellationRequested();
            var maxDiscount = activePromos.Where(p => p.ProductSku == product.Sku)
                                          .Select(x => x.DiscountPercent)
                                          .DefaultIfEmpty(0m)
                                          .Max();
            var newPrice = Math.Round(product.BasePrice * (1 - maxDiscount), 2, MidpointRounding.AwayFromZero);
            if (newPrice != product.CurrentPrice)
            {
                product.CurrentPrice = newPrice;
                productsUpdated++;
            }
            promosApplied += activePromos.Count(p => p.ProductSku == product.Sku);
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Recalculo de precios: {Updated} productos, {Promos} promos.", productsUpdated, promosApplied);
        return new PriceCalcResult(productsUpdated, promosApplied);
    }
}
