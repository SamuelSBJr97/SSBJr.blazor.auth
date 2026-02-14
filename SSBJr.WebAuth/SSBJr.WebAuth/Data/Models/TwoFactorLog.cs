using System.ComponentModel.DataAnnotations;

namespace SSBJr.WebAuth.Data.Models;

public class TwoFactorLog
{
    [Key]
    public Guid LogId { get; set; }

    public Guid UserId { get; set; }

    [Required]
    [StringLength(6)]
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
