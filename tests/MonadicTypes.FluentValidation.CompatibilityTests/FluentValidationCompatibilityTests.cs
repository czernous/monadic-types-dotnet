using FluentValidation;
using FluentValidation.Results;
using MonadicTypes;

namespace MonadicTypes.FluentValidation.CompatibilityTests;

public sealed class FluentValidationCompatibilityTests
{
    [Fact]
    public void GenericMapperPreservesStandardFailureDataWithoutRuntimeAdapter()
    {
        ValidationResult validation = new CustomerValidator().Validate(new Customer(string.Empty));

        ValidationErrors errors = ValidationErrors.Create(validation.Errors, static failure =>
            new ValidationIssue(
                failure.PropertyName,
                failure.ErrorCode,
                failure.ErrorMessage,
                failure.Severity switch
                {
                    Severity.Error => ValidationSeverity.Error,
                    Severity.Warning => ValidationSeverity.Warning,
                    Severity.Info => ValidationSeverity.Information,
                    _ => throw new ArgumentOutOfRangeException(nameof(failure))
                }));

        ValidationIssue issue = Assert.Single(errors);
        Assert.Equal("Email", issue.Path);
        Assert.Equal("EMAIL_REQUIRED", issue.Code);
        Assert.Equal("Email is required.", issue.Message);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
    }

    [Fact]
    public async Task AsyncValidationPreservesCancellationWithoutAdapterOwnership()
    {
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new CustomerValidator().ValidateAsync(new Customer(string.Empty), cancellation.Token));
    }

    private sealed record Customer(string Email);

    private sealed class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(customer => customer.Email)
                .NotEmpty()
                .WithErrorCode("EMAIL_REQUIRED")
                .WithMessage("Email is required.");
        }
    }
}
