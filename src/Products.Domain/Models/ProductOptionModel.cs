namespace Products.Domain.Models;

public class ProductOptionModel
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }
}
