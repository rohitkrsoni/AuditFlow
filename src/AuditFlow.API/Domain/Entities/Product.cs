using AuditFlow.API.Domain.Common;
using AuditFlow.API.Domain.Enums;

namespace AuditFlow.API.Domain.Entities;

public readonly record struct ProductId(Guid Value);

public class Product : BaseEntity
{
    protected Product()
    { }

    protected Product(string name, double price, string description, string imageUrl, Category category, Size size)
    {
        Id = new ProductId(Guid.CreateVersion7());
        Name = name;
        Price = price;
        Description = description;
        ImageUrl = imageUrl;
        Category = category;
        Size = size;
    }

    public ProductId Id { get; private set; }
    public string Name { get; private set; }
    public double Price { get; private set; }
    public string Description { get; private set; }
    public string ImageUrl { get; private set; }
    public Category Category { get; private set; }
    public Size Size { get; private set; }

    public static Product Create(string name, double price, string description, string imageUrl, Category category, Size size)
    {
        return new Product(name, price, description, imageUrl, category, size);
    }
    public void Update(string name, double price, string description, string imageUrl, Category category, Size size)
    {
        Name = name;
        Price = price;
        Description = description;
        ImageUrl = imageUrl;
        Category = category;
        Size = size;
    }
}
