using System.ComponentModel.DataAnnotations;

namespace SSBJr.WebAuth.Data.Models;

public class AuditLog
{
    [Key]
    public Guid AuditId { get; set; }

    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
