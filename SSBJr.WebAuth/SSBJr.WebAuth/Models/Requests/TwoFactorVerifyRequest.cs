namespace SSBJr.WebAuth.Models.Requests;

public class TwoFactorVerifyRequest
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}
