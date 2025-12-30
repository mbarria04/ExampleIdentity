
using System;
using System.Collections.Concurrent;
using CapaData.Models;

namespace PriceRecalculationMvc_VS2022.Services;

public interface IJobsStore
{
    void Insert(PriceJob job);
    void ChangeState(Guid id, JobState state, string? error = null);
    bool TryGet(Guid id, out (PriceJob job, JobState state, string? error) info);
}

public class JobsStore : IJobsStore
{
    private readonly ConcurrentDictionary<Guid, (PriceJob, JobState, string?)> _dic = new();

    public void Insert(PriceJob job) => _dic[job.Id] = (job, JobState.Pending, null);

    public void ChangeState(Guid id, JobState state, string? error = null)
    {
        if (_dic.TryGetValue(id, out var current))
            _dic[id] = (current.Item1, state, error);
    }

    public bool TryGet(Guid id, out (PriceJob, JobState, string?) info) => _dic.TryGetValue(id, out info);
}
