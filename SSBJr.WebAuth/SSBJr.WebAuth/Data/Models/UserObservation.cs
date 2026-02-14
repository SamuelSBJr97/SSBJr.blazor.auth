using System.ComponentModel.DataAnnotations;

namespace SSBJr.WebAuth.Data.Models;

public class UserObservation
{
    [Key]
    public Guid ObservationId { get; set; }

    public Guid UserId { get; set; }

    public Guid GestorId { get; set; }

    [Required]
    [StringLength(2000)]
    public string Observation { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser? User { get; set; }

    public ApplicationUser? Gestor { get; set; }
}
