using System.ComponentModel.DataAnnotations;

namespace Products.Application.DTOs;

public class ProductOptionDTO
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Product option name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product option name must be between 1 and 100 characters")]
    public required string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
