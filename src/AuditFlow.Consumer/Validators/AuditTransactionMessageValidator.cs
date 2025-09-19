using AuditFlow.Shared.AuditContracts;
using FluentValidation;
using MassTransit.Serialization;

namespace AuditFlow.Consumer.Validators;

public sealed class AuditTransactionMessageValidator
    : AbstractValidator<AuditTransactionMessage>
{
    public AuditTransactionMessageValidator()
    {
        RuleFor(x => x.EventId)
            .NotNull()
            .NotEmpty().WithMessage("EventId is required.");

        RuleFor(x => x.IdentityUserId)
            .NotNull()
            .NotEmpty().WithMessage("IdentityUserId is required.");

        RuleFor(x => x.TransactionDetails)
            .NotNull()
            .NotEmpty().WithMessage("At least one TransactionDetail is required.");

        RuleForEach(x => x.TransactionDetails!)
            .SetValidator(new AuditTransactionDetailMessageValidator());
    }
}
