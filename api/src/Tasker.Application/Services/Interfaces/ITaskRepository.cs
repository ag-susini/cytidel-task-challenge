using Tasker.Domain.Entities;

namespace Tasker.Application.Services.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task AddAsync(TaskItem taskItem);
    Task UpdateAsync(TaskItem taskItem);
    Task DeleteAsync(TaskItem taskItem);
    Task SaveChangesAsync();
}