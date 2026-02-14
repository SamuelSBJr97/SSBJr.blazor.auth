namespace SSBJr.WebAuth.Models.Requests;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}
