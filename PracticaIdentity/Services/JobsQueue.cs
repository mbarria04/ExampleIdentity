
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CapaData.Models;

namespace PriceRecalculationMvc_VS2022.Services;

public class JobsQueue : IJobsQueue
{
    private readonly Channel<PriceJob> _channel;
    private readonly ILogger<JobsQueue> _logger;

    public JobsQueue(IConfiguration config, ILogger<JobsQueue> logger)
    {
        _logger = logger;
        var capacity = config.GetValue<int?>("Jobs:QueueCapacity") ?? 1000;
        var opts = new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<PriceJob>(opts);
        _logger.LogInformation("Cola de jobs creada con capacidad {Capacity}", capacity);
    }

    public ValueTask EnqueueAsync(PriceJob job, CancellationToken ct)
    {
        _logger.LogInformation("Encolando job {Id} (razón: {Reason})", job.Id, job.Reason);
        return _channel.Writer.WriteAsync(job, ct);
    }

    public async IAsyncEnumerable<PriceJob> ReadAllAsync(CancellationToken ct)
    {
        while (await _channel.Reader.WaitToReadAsync(ct))
        {
            while (_channel.Reader.TryRead(out var item))
                yield return item;
        }
    }
}
