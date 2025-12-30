
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CapaData.Models;

namespace PriceRecalculationMvc_VS2022.Services;

public interface IJobsQueue
{
    ValueTask EnqueueAsync(PriceJob job, CancellationToken ct);
    IAsyncEnumerable<PriceJob> ReadAllAsync(CancellationToken ct);
}
