using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Dtos;

public class ContactDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? Phone { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    
    [Required]
    [MinLength(20)]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}