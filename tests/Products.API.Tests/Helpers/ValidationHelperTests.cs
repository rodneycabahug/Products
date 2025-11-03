using System.ComponentModel.DataAnnotations;
using Products.API.Helpers;
using TUnit.Core;

namespace Products.API.Tests.Helpers;

public class ValidationHelperTests
{
    private class TestDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 1000, ErrorMessage = "Price must be between 0.01 and 1000")]
        public decimal Price { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
    }

    [Test]
    public async Task ValidateObject_ValidObject_ReturnsTrue()
    {
        // Arrange
        var validObject = new TestDto
        {
            Name = "Valid Name",
            Price = 50m,
            Email = "test@example.com"
        };

        // Act
        var (isValid, errors) = ValidationHelper.ValidateObject(validObject);

        // Assert
        await Assert.That(isValid).IsTrue();
        await Assert.That(errors).IsEmpty();
    }

    [Test]
    public async Task ValidateObject_MissingRequiredField_ReturnsFalse()
    {
        // Arrange
        var invalidObject = new TestDto
        {
            Name = "", // Required field empty
            Price = 50m
        };

        // Act
        var (isValid, errors) = ValidationHelper.ValidateObject(invalidObject);

        // Assert
        await Assert.That(isValid).IsFalse();
        await Assert.That(errors).IsNotEmpty();
        await Assert.That(errors.Any(e => e.Contains("Name is required"))).IsTrue();
    }

    [Test]
    public async Task ValidateObject_InvalidRange_ReturnsFalse()
    {
        // Arrange
        var invalidObject = new TestDto
        {
            Name = "Valid Name",
            Price = 0m // Below minimum
        };

        // Act
        var (isValid, errors) = ValidationHelper.ValidateObject(invalidObject);

        // Assert
        await Assert.That(isValid).IsFalse();
        await Assert.That(errors).IsNotEmpty();
        await Assert.That(errors.Any(e => e.Contains("Price must be between"))).IsTrue();
    }

    [Test]
    public async Task ValidateObject_InvalidStringLength_ReturnsFalse()
    {
        // Arrange
        var invalidObject = new TestDto
        {
            Name = "A", // Too short (minimum 2)
            Price = 50m
        };

        // Act
        var (isValid, errors) = ValidationHelper.ValidateObject(invalidObject);

        // Assert
        await Assert.That(isValid).IsFalse();
        await Assert.That(errors).IsNotEmpty();
        await Assert.That(errors.Any(e => e.Contains("Name must be between"))).IsTrue();
    }

    [Test]
    public async Task ValidateObject_InvalidEmail_ReturnsFalse()
    {
        // Arrange
        var invalidObject = new TestDto
        {
            Name = "Valid Name",
            Price = 50m,
            Email = "not-an-email" // Invalid email format
        };

        // Act
        var (isValid, errors) = ValidationHelper.ValidateObject(invalidObject);

        // Assert
        await Assert.That(isValid).IsFalse();
        await Assert.That(errors).IsNotEmpty();
        await Assert.That(errors.Any(e => e.Contains("Invalid email"))).IsTrue();
    }

    [Test]
    public async Task ValidateObject_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var invalidObject = new TestDto
        {
            Name = "", // Required error
            Price = -10m, // Range error
            Email = "invalid" // Email error
        };

        // Act
        var (isValid, errors) = ValidationHelper.ValidateObject(invalidObject);

        // Assert
        await Assert.That(isValid).IsFalse();
        await Assert.That(errors.Count).IsGreaterThanOrEqualTo(2); // At least 2 errors
    }

    [Test]
    public async Task ValidateObject_NullOptionalField_IsValid()
    {
        // Arrange
        var validObject = new TestDto
        {
            Name = "Valid Name",
            Price = 50m,
            Email = null // Optional field
        };

        // Act
        var (isValid, errors) = ValidationHelper.ValidateObject(validObject);

        // Assert
        await Assert.That(isValid).IsTrue();
        await Assert.That(errors).IsEmpty();
    }
}
