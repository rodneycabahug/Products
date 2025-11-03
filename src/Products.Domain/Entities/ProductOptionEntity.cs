namespace Products.Domain.Entities;

public class ProductOptionEntity : BaseEntity
{
    public override Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
}
