using FluentValidation;
using Tasker.Application.Commands;

namespace Tasker.Application.Validators;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskItemCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Task ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Invalid priority value");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid status value");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.Now.AddDays(-1))
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date cannot be in the far past");
    }
}