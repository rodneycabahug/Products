namespace Products.Domain.Entities;

public class ProductEntity : BaseEntity
{
    public override Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal DeliveryPrice { get; set; }
}
