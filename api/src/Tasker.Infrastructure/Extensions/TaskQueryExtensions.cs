using Tasker.Domain.Entities;
using Tasker.Domain.Enums;

namespace Tasker.Infrastructure.Extensions;

public static class TaskQueryExtensions
{
    public static IQueryable<TaskItem> FilterByStatus(this IQueryable<TaskItem> query, Status? status)
    {
        return status.HasValue ? query.Where(t => t.Status == status.Value) : query;
    }

    public static IQueryable<TaskItem> FilterByPriority(this IQueryable<TaskItem> query, Priority? priority)
    {
        return priority.HasValue ? query.Where(t => t.Priority == priority.Value) : query;
    }

    public static IQueryable<TaskItem> FilterBySearch(this IQueryable<TaskItem> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        return query.Where(t => t.Title.Contains(search) || 
                              (t.Description != null && t.Description.Contains(search)));
    }

    public static IQueryable<TaskItem> ApplySort(this IQueryable<TaskItem> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderByDescending(t => t.CreatedAt);

        return sort.ToLower() switch
        {
            "title" => query.OrderBy(t => t.Title),
            "title_desc" => query.OrderByDescending(t => t.Title),
            "priority" => query.OrderBy(t => t.Priority),
            "priority_desc" => query.OrderByDescending(t => t.Priority),
            "status" => query.OrderBy(t => t.Status),
            "status_desc" => query.OrderByDescending(t => t.Status),
            "duedate" => query.OrderBy(t => t.DueDate),
            "duedate_desc" => query.OrderByDescending(t => t.DueDate),
            "created" => query.OrderBy(t => t.CreatedAt),
            "created_desc" => query.OrderByDescending(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }
}