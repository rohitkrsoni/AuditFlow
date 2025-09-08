using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;

using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace AuditFlow.API.Features.Products.UpdateProduct;

public sealed record UpdateProductRequest
{
    [FromRoute(Name = "id")] public Guid Id { get; set; }
    [FromBody] public CreateUpdateProductRequest Request { get; set; }
}

internal sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Request)
            .SetValidator(new CreateUpdateProductRequestValidator());
    }
}
