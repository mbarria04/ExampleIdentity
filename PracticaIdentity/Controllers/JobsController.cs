using CapaData.Models;
using Microsoft.AspNetCore.Mvc;
using PriceRecalculationMvc_VS2022.Services;
namespace PracticaIdentity.Controllers
{
    public class JobsController : ControllerBase
    {
        private readonly IJobsQueue _queue;
        private readonly IJobsStore _store;
        private readonly ILogger<JobsController> _logger;

        public JobsController(IJobsQueue queue, IJobsStore store, ILogger<JobsController> logger)
        { _queue = queue; _store = store; _logger = logger; }

        public record RecalcPricesDto(string? Reason);

        [HttpPost("recalc-prices")]
        public async Task<IActionResult> EnqueueRecalc([FromBody] RecalcPricesDto dto, CancellationToken ct)
        {
            var job = new PriceJob(Guid.NewGuid(), DateTimeOffset.Now, dto.Reason ?? "Manual");
            _store.Insert(job);
            await _queue.EnqueueAsync(job, ct);
            _logger.LogInformation("Job {Id} encolado manualmente.", job.Id);
            return AcceptedAtAction(nameof(Status), new { id = job.Id }, new { id = job.Id });
        }

        [HttpGet("{id:guid}")]
        public IActionResult Status(Guid id)
        {
            if (_store.TryGet(id, out var info))
            {
                var (job, state, error) = info;
                return Ok(new { id = job.Id, reason = job.Reason, created = job.Created, state = state.ToString(), error });
            }
            return NotFound(new { error = "Job no encontrado." });
        }
    }
}