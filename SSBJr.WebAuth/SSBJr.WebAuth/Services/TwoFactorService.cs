using System.Security.Cryptography;

namespace SSBJr.WebAuth.Services;

public interface ITwoFactorService
{
    string GenerateCode();
    DateTime GetExpirationTime(int durationMinutes = 15);
    bool IsCodeExpired(DateTime expiresAt);
}

public class TwoFactorService : ITwoFactorService
{
    public string GenerateCode()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] buffer = new byte[4];
            rng.GetBytes(buffer);
            uint random = BitConverter.ToUInt32(buffer, 0) % 1000000;
            return random.ToString("D6");
        }
    }

    public DateTime GetExpirationTime(int durationMinutes = 15)
    {
        return DateTime.UtcNow.AddMinutes(durationMinutes);
    }

    public bool IsCodeExpired(DateTime expiresAt)
    {
        return DateTime.UtcNow > expiresAt;
    }
}
