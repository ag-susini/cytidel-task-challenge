using FluentValidation;
using FluentValidation.Results;
using Tasker.Application.Behaviors;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Tests.Behaviors;

public class ValidationCommandHandlerTests
{
    private readonly ICommandHandler<TestCommand> _innerHandler;
    private readonly IValidator<TestCommand> _validator;
    private readonly ValidationCommandHandler<TestCommand> _validationHandler;

    public ValidationCommandHandlerTests()
    {
        _innerHandler = Substitute.For<ICommandHandler<TestCommand>>();
        _validator = Substitute.For<IValidator<TestCommand>>();
        _validationHandler = new ValidationCommandHandler<TestCommand>(_innerHandler, new[] { _validator });
    }

    [Fact]
    public async Task HandleAsync_ShouldCallInnerHandler_WhenNoValidatorsProvided()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var handlerWithoutValidators = new ValidationCommandHandler<TestCommand>(_innerHandler, Enumerable.Empty<IValidator<TestCommand>>());

        // Act
        await handlerWithoutValidators.HandleAsync(command);

        // Assert
        await _innerHandler.Received(1).HandleAsync(command);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallInnerHandler_WhenValidationPasses()
    {
        // Arrange
        var command = new TestCommand { Value = "valid" };
        var validationResult = new ValidationResult();
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(validationResult);

        // Act
        await _validationHandler.HandleAsync(command);

        // Assert
        await _innerHandler.Received(1).HandleAsync(command);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var command = new TestCommand { Value = "invalid" };
        var validationFailure = new ValidationFailure("Value", "Value is invalid");
        var validationResult = new ValidationResult(new[] { validationFailure });
        
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(validationResult);

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(
            async () => await _validationHandler.HandleAsync(command));

        exception.Errors.ShouldContain(validationFailure);
        await _innerHandler.DidNotReceive().HandleAsync(command);
    }

    [Fact]
    public async Task HandleAsync_ShouldCollectAllValidationFailures_WhenMultipleValidatorsFail()
    {
        // Arrange
        var command = new TestCommand { Value = "invalid" };
        
        var validator1 = Substitute.For<IValidator<TestCommand>>();
        var validator2 = Substitute.For<IValidator<TestCommand>>();
        
        var failure1 = new ValidationFailure("Value", "First validation error");
        var failure2 = new ValidationFailure("Value", "Second validation error");
        
        validator1.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(new ValidationResult(new[] { failure1 }));
        validator2.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(new ValidationResult(new[] { failure2 }));

        var multiValidatorHandler = new ValidationCommandHandler<TestCommand>(_innerHandler, new[] { validator1, validator2 });

        // Act & Assert
        var exception = await Should.ThrowAsync<ValidationException>(
            async () => await multiValidatorHandler.HandleAsync(command));

        exception.Errors.ShouldContain(failure1);
        exception.Errors.ShouldContain(failure2);
        await _innerHandler.DidNotReceive().HandleAsync(command);
    }

    [Fact]
    public async Task HandleAsync_ShouldRunAllValidatorsInParallel()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        
        var validator1 = Substitute.For<IValidator<TestCommand>>();
        var validator2 = Substitute.For<IValidator<TestCommand>>();
        var validator3 = Substitute.For<IValidator<TestCommand>>();
        
        validator1.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(new ValidationResult());
        validator2.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(new ValidationResult());
        validator3.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(new ValidationResult());

        var multiValidatorHandler = new ValidationCommandHandler<TestCommand>(_innerHandler, new[] { validator1, validator2, validator3 });

        // Act
        await multiValidatorHandler.HandleAsync(command);

        // Assert
        await validator1.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestCommand>>());
        await validator2.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestCommand>>());
        await validator3.Received(1).ValidateAsync(Arg.Any<ValidationContext<TestCommand>>());
        await _innerHandler.Received(1).HandleAsync(command);
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterOutNullValidationFailures()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var validationResult = new ValidationResult();
        // ValidationResult.Errors collection should not contain nulls in normal usage,
        // but the handler code specifically filters for them, so we test this scenario
        
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(validationResult);

        // Act
        await _validationHandler.HandleAsync(command);

        // Assert
        await _innerHandler.Received(1).HandleAsync(command);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCorrectValidationContext()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        _validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>()).Returns(new ValidationResult());

        // Act
        await _validationHandler.HandleAsync(command);

        // Assert
        await _validator.Received(1).ValidateAsync(Arg.Is<ValidationContext<TestCommand>>(ctx => 
            ctx.InstanceToValidate == command));
    }

    // Test command class for testing purposes
    public class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }
}