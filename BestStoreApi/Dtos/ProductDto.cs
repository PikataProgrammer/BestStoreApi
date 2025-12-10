using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Dtos;

public class ProductDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;
    [Required]
    public decimal Price { get; set; }
    [MaxLength(4000)]
    public string? Description { get; set; } = string.Empty;

    public IFormFile? ImageFile { get; set; }
}