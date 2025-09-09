using FluentValidation.TestHelper;
using Tasker.Application.Commands;
using Tasker.Application.Validators;
using Tasker.Domain.Enums;

namespace Tasker.Application.Tests.Validators;

public class UpdateTaskCommandValidatorTests
{
    private readonly UpdateTaskCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateTaskItemCommand(
            Guid.Empty,
            "Title",
            "Description",
            Priority.Medium,
            Status.Pending,
            DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Task ID is required");
    }

    [Fact]
    public void Should_NotHaveError_WhenIdIsValid()
    {
        // Arrange
        var command = new UpdateTaskItemCommand(
            Guid.NewGuid(),
            "Title",
            "Description",
            Priority.Medium,
            Status.Pending,
            DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(Status.Pending)]
    [InlineData(Status.InProgress)]
    [InlineData(Status.Completed)]
    [InlineData(Status.Archived)]
    public void Should_NotHaveError_WhenStatusIsValid(Status status)
    {
        // Arrange
        var command = new UpdateTaskItemCommand(
            Guid.NewGuid(),
            "Title",
            "Description",
            Priority.Medium,
            status,
            DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_HaveError_WhenStatusIsInvalid()
    {
        // Arrange
        var command = new UpdateTaskItemCommand(
            Guid.NewGuid(),
            "Title",
            "Description",
            Priority.Medium,
            (Status)999, // Invalid status
            DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Invalid status value");
    }

    [Fact]
    public void Should_PassValidation_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new UpdateTaskItemCommand(
            Guid.NewGuid(),
            "Valid Title",
            "Valid Description",
            Priority.High,
            Status.InProgress,
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
        var command = new UpdateTaskItemCommand(
            Guid.Empty, // Empty ID
            "", // Empty title
            new string('a', 2001), // Too long description
            (Priority)999, // Invalid priority
            (Status)999, // Invalid status
            DateTime.Now.AddDays(-10)); // Past date

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(6);
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.Priority);
        result.ShouldHaveValidationErrorFor(x => x.Status);
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Should_InheritCommonValidationRules_FromBaseProperties()
    {
        // This test verifies that UpdateTaskCommandValidator includes all the same
        // validation rules as CreateTaskCommandValidator for common properties
        
        // Arrange
        var command = new UpdateTaskItemCommand(
            Guid.NewGuid(),
            new string('a', 201), // Title too long
            "Description",
            Priority.Medium,
            Status.Pending,
            DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Should_AllowNullDueDate()
    {
        // Arrange
        var command = new UpdateTaskItemCommand(
            Guid.NewGuid(),
            "Title",
            "Description",
            Priority.Medium,
            Status.Pending,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Should_HaveError_WhenTitleIsEmptyOrWhitespace(string title)
    {
        // Arrange
        var command = new UpdateTaskItemCommand(
            Guid.NewGuid(),
            title,
            "Description",
            Priority.Medium,
            Status.Pending,
            DateTime.Now);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }
}