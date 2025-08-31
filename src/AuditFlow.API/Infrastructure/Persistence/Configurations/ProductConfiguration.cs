using AuditFlow.API.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditFlow.API.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(u => u.Id);

        builder
            .Property(o => o.Id)
            .HasConversion(Id => Id.Value,
                value => new ProductId(value))
            .ValueGeneratedNever();
    }
}
