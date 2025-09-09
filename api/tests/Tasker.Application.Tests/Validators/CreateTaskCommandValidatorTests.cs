using FluentValidation.TestHelper;
using Tasker.Application.Commands;
using Tasker.Application.Validators;
using Tasker.Domain.Enums;

namespace Tasker.Application.Tests.Validators;

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateTaskItemCommand("", "Description", Priority.Medium, DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Should_HaveError_WhenTitleIsNull()
    {
        // Arrange
        var command = new CreateTaskItemCommand(null!, "Description", Priority.Medium, DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Should_HaveError_WhenTitleExceeds200Characters()
    {
        // Arrange
        var longTitle = new string('a', 201);
        var command = new CreateTaskItemCommand(longTitle, "Description", Priority.Medium, DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Should_NotHaveError_WhenTitleIs200CharactersExactly()
    {
        // Arrange
        var maxTitle = new string('a', 200);
        var command = new CreateTaskItemCommand(maxTitle, "Description", Priority.Medium, DateTime.Now.AddDays(1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_HaveError_WhenDescriptionExceeds2000Characters()
    {
        // Arrange
        var longDescription = new string('a', 2001);
        var command = new CreateTaskItemCommand("Title", longDescription, Priority.Medium, DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 2000 characters");
    }

    [Fact]
    public void Should_NotHaveError_WhenDescriptionIsNull()
    {
        // Arrange
        var command = new CreateTaskItemCommand("Title", null, Priority.Medium, DateTime.Now.AddDays(1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_NotHaveError_WhenDescriptionIs2000CharactersExactly()
    {
        // Arrange
        var maxDescription = new string('a', 2000);
        var command = new CreateTaskItemCommand("Title", maxDescription, Priority.Medium, DateTime.Now.AddDays(1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(-30)]
    [InlineData(-365)]
    public void Should_HaveError_WhenDueDateIsInTheFarPast(int daysInPast)
    {
        // Arrange
        var pastDate = DateTime.Now.AddDays(daysInPast);
        var command = new CreateTaskItemCommand("Title", "Description", Priority.Medium, pastDate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DueDate)
            .WithErrorMessage("Due date cannot be in the far past");
    }

    [Fact]
    public void Should_NotHaveError_WhenDueDateIsToday()
    {
        // Arrange
        var command = new CreateTaskItemCommand("Title", "Description", Priority.Medium, DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Should_NotHaveError_WhenDueDateIsInFuture()
    {
        // Arrange
        var futureDate = DateTime.Now.AddDays(7);
        var command = new CreateTaskItemCommand("Title", "Description", Priority.Medium, futureDate);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Should_NotHaveError_WhenDueDateIsNull()
    {
        // Arrange
        var command = new CreateTaskItemCommand("Title", "Description", Priority.Medium, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    public void Should_NotHaveError_WhenPriorityIsValid(Priority priority)
    {
        // Arrange
        var command = new CreateTaskItemCommand("Title", "Description", priority, DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Priority);
    }

    [Fact]
    public void Should_HaveError_WhenPriorityIsInvalid()
    {
        // Arrange - Using reflection to create an invalid enum value
        var command = new CreateTaskItemCommand("Title", "Description", (Priority)999, DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Priority)
            .WithErrorMessage("Invalid priority value");
    }

    [Fact]
    public void Should_PassValidation_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateTaskItemCommand(
            "Valid Title",
            "Valid Description",
            Priority.High,
            DateTime.Now.AddDays(7));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_HaveMultipleErrors_WhenMultipleFieldsAreInvalid()
    {
        // Arrange
        var command = new CreateTaskItemCommand(
            "", // Empty title
            new string('a', 2001), // Too long description
            Priority.Medium,
            DateTime.Now.AddDays(-10)); // Past date

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(3);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }
}