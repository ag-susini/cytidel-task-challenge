using Microsoft.EntityFrameworkCore;
using Tasker.Application.Services.Interfaces;
using Tasker.Infrastructure.Persistence;

namespace Tasker.Infrastructure.Repositories;

public class TaskItemRepository : ITaskRepository
{
    private readonly TaskDbContext _dbContext;

    public TaskItemRepository(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Domain.Entities.TaskItem?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(Domain.Entities.TaskItem taskItem)
    {
        await _dbContext.Tasks.AddAsync(taskItem);
    }

    public Task UpdateAsync(Domain.Entities.TaskItem taskItem)
    {
        _dbContext.Tasks.Update(taskItem);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.TaskItem taskItem)
    {
        _dbContext.Tasks.Remove(taskItem);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}