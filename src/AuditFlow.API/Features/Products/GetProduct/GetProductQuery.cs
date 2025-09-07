using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;

using FluentValidation;

namespace AuditFlow.API.Features.Products.GetProduct;


public sealed record GetProductQuery(ProductId Id) : IQuery<CreateUpdateProductResponse>;

internal sealed class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryValidator()
    {
        RuleFor(x => x.Id.Value).NotEmpty();
    }
}
