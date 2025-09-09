using FluentValidation;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Behaviors;

public class ValidationCommandHandler<TCommand> : ICommandHandler<TCommand> 
    where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public ValidationCommandHandler(ICommandHandler<TCommand> handler, IEnumerable<IValidator<TCommand>> validators)
    {
        _handler = handler;
        _validators = validators;
    }

    public async Task HandleAsync(TCommand command)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TCommand>(command);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context)));
            
            var failures = validationResults
                .SelectMany(result => result.Errors)
                .Where(error => error != null)
                .ToList();

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }

        await _handler.HandleAsync(command);
    }
}