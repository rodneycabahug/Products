using System.ComponentModel.DataAnnotations;

namespace Products.Application.DTOs;

public class ProductDTO
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 100 characters")]
    public required string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Delivery price must be greater than 0")]
    public decimal DeliveryPrice { get; set; }
}
