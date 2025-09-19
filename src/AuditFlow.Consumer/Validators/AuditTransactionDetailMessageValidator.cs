using FluentValidation;
using AuditFlow.Shared.AuditContracts;

namespace AuditFlow.Consumer.Validators;

public sealed class AuditTransactionDetailMessageValidator
    : AbstractValidator<AuditTransactionDetailMessage>
{
    public AuditTransactionDetailMessageValidator()
    {
        RuleFor(d => d.EntityName)
            .NotNull()
            .NotEmpty().WithMessage("EntityName is required.");

        RuleFor(d => d.PropertyName)
            .NotNull()
            .NotEmpty().WithMessage("PropertyName is required.");

        RuleFor(d => d.PrimaryKeyValue)
            .NotNull()
            .NotEmpty().WithMessage("PrimaryKeyValue is required.");

        RuleFor(d => d.DataAuditTransactionType)
            .NotNull()
            .WithMessage("DataAuditTransactionType must be 1,2,3, or 4.");
    }
}
