
using CapaData.Data;
using CapaData.Entities;
using CapaData.Models;
using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace PriceRecalculationMvc_VS2022.Services;

public interface IJobsStore
{
    void Insert(PriceJob job);
    void ChangeState(Guid id, JobState state, string? error = null);
    bool TryGet(Guid id, out (PriceJob job, JobState state, string? error) info);
}



public class JobsStore : IJobsStore
{
    private readonly IDbContextFactory<DB_Context> _dbFactory;
    private readonly ILogger<JobsStore> _logger;

    public JobsStore(IDbContextFactory<DB_Context> dbFactory, ILogger<JobsStore> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public void Insert(PriceJob job)
    {
        using var db = _dbFactory.CreateDbContext();

        var entity = new JobEntity
        {
            Id = job.Id,
            Created = job.Created,
            Reason = job.Reason,
            State = "Pending",
            Error = null
        };

        db.Jobs.Add(entity);
        db.SaveChanges();
        _logger.LogInformation("Job {Id} insertado en BD (State=Pending)", job.Id);
    }

    public void ChangeState(Guid id, JobState state, string? error = null)
    {
        using var db = _dbFactory.CreateDbContext();

        var entity = db.Jobs.Find(id);
        if (entity is null)
        {
            _logger.LogWarning("Job {Id} no existe en BD", id);
            return;
        }

        entity.State = state switch
        {
            JobState.Pending => "Pending",
            JobState.Processing => "Processing",
            JobState.Completed => "Completed",
            JobState.Error => "Failed", // si tu enum usa Error para fallas
            _ => "Pending"
        };
        entity.Error = error;

        db.SaveChanges();
        _logger.LogInformation("Job {Id} actualizado a {State}", id, entity.State);
    }

    public bool TryGet(Guid id, out (PriceJob job, JobState state, string? error) info)
    {
        using var db = _dbFactory.CreateDbContext();

        var e = db.Jobs.AsNoTracking().FirstOrDefault(x => x.Id == id);
        if (e is null)
        {
            info = default;
            return false;
        }

        var job = new PriceJob(e.Id, e.Created, e.Reason);
        var state = e.State?.ToLowerInvariant() switch
        {
            "pending" => JobState.Pending,
            "processing" => JobState.Processing,
            "completed" => JobState.Completed,
            "failed" => JobState.Error, // o Failed, según tu enum
            _ => JobState.Pending
        };
        info = (job, state, e.Error);
        return true;
    }
}


