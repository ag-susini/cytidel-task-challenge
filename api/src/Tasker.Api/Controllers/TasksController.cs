using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using Tasker.Application.Commands;
using Tasker.Application.DTOs;
using Tasker.Application.Queries;
using Tasker.Shared.Abstractions.Commands;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Api.Controllers;

[Authorize]
[SwaggerTag("Task management endpoints")]
public class TasksController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    : BaseController
{
    [HttpGet("{id:guid}")]
    [EnableRateLimiting("ApiPolicy")]
    [SwaggerOperation(Summary = "Get task by ID", Description = "Retrieves a specific task by its unique identifier")]
    [SwaggerResponse(200, "Task retrieved successfully", typeof(TaskDto))]
    [SwaggerResponse(404, "Task not found")]
    public async Task<ActionResult<TaskDto>> Get([FromRoute] GetTaskItemByIdQuery query)
    {
        var result = await queryDispatcher.QueryAsync(query);
        return OkOrNotFound(result);
    }

    [HttpGet]
    [EnableRateLimiting("ApiPolicy")]
    [SwaggerOperation(Summary = "Get paginated tasks with filtering", Description = "Retrieves tasks with optional filtering by status, priority, and search term")]
    [SwaggerResponse(200, "Tasks retrieved successfully", typeof(PagedResult<TaskListItem>))]
    public async Task<ActionResult<PagedResult<TaskListItem>>> Get([FromQuery] GetTaskItemsPagedQuery query)
    {
        var result = await queryDispatcher.QueryAsync(query);
        return Ok(result);
    }

    [HttpGet("stats")]
    [EnableRateLimiting("StatsPolicy")]
    [SwaggerOperation(Summary = "Get task statistics", Description = "Retrieves task count statistics grouped by status")]
    [SwaggerResponse(200, "Statistics retrieved successfully", typeof(TaskStatsDto))]
    public async Task<ActionResult<TaskStatsDto>> GetStats()
    {
        var query = new GetTaskItemStatsQuery();
        var result = await queryDispatcher.QueryAsync(query);
        return Ok(result);
    }

    [HttpPost]
    [EnableRateLimiting("WritePolicy")]
    [SwaggerOperation(Summary = "Create a new task", Description = "Creates a new task with the specified details")]
    [SwaggerResponse(201, "Task created successfully", typeof(TaskDto))]
    [SwaggerResponse(400, "Invalid task data")]
    public async Task<ActionResult<TaskDto>> Post([FromBody] CreateTaskItemCommand command)
    {
        var createdTask = await commandDispatcher.DispatchAsync<TaskDto>(command);
        return CreatedAtAction(nameof(Get), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id:guid}")]
    [EnableRateLimiting("WritePolicy")]
    [SwaggerOperation(Summary = "Update an existing task", Description = "Updates all properties of an existing task")]
    [SwaggerResponse(200, "Task updated successfully")]
    [SwaggerResponse(404, "Task not found")]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] UpdateTaskItemCommand command)
    {
        // Ensure the ID from route is used
        command = command with { Id = id };
        
        try
        {
            await commandDispatcher.DispatchAsync(command);
            return Ok();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    [EnableRateLimiting("WritePolicy")]
    [SwaggerOperation(Summary = "Delete a task", Description = "Permanently deletes a task by its unique identifier")]
    [SwaggerResponse(204, "Task deleted successfully")]
    [SwaggerResponse(404, "Task not found")]
    public async Task<IActionResult> Delete([FromRoute] DeleteTaskItemCommand command)
    {
        try
        {
            await commandDispatcher.DispatchAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound();
        }
    }
}