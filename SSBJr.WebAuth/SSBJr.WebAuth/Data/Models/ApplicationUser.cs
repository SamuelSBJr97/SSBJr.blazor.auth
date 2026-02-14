using System.ComponentModel.DataAnnotations;

namespace SSBJr.WebAuth.Data.Models;

public class ApplicationUser
{
    [Key]
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    [Required]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(255)]
    public string? FirstName { get; set; }

    [StringLength(255)]
    public string? LastName { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? ProfessionalTitle { get; set; }

    [StringLength(500)]
    public string? Department { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(2)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? ZipCode { get; set; }

    /// <summary>
    /// Admin, User, Gestor
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "User";

    public bool IsTwoFactorEnabled { get; set; } = true;

    [StringLength(500)]
    public string? TwoFactorSecret { get; set; }

    public bool IsBlocked { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public Tenant? Tenant { get; set; }

    public ICollection<TwoFactorLog> TwoFactorLogs { get; set; } = [];

    public ICollection<UserObservation> UserObservations { get; set; } = [];

    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
