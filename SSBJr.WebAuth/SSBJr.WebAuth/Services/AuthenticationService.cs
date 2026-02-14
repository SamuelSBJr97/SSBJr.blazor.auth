using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SSBJr.WebAuth.Data;
using SSBJr.WebAuth.Data.Models;

namespace SSBJr.WebAuth.Services;

public interface IAuthenticationService
{
    Task<(bool Success, string Message, ApplicationUser? User)> RegisterUserAsync(string email, string password, Guid tenantId, string firstName, string lastName);
    Task<(bool Success, string Message, ApplicationUser? User)> LoginUserAsync(string email, string password, Guid tenantId);
    Task<(bool Success, string Message)> VerifyTwoFactorCodeAsync(Guid userId, string code);
    Task<(string Code, DateTime ExpiresAt)> GenerateTwoFactorCodeAsync(Guid userId);
    Task<bool> PromoteToGestorAsync(Guid userId, Guid tenantId);
    Task<bool> PromoteToAdminAsync(Guid userId, Guid tenantId);
    Task<bool> BlockUserAsync(Guid userId, Guid tenantId);
    Task<bool> UnblockUserAsync(Guid userId, Guid tenantId);
    Task<bool> SoftDeleteUserAsync(Guid userId, Guid tenantId);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        AppDbContext context,
        IPasswordService passwordService,
        ITwoFactorService twoFactorService,
        IEmailService emailService,
        ILogger<AuthenticationService> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _twoFactorService = twoFactorService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, ApplicationUser? User)> RegisterUserAsync(
        string email, string password, Guid tenantId, string firstName, string lastName)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null || !tenant.IsActive)
                return (false, "Tenant não encontrado ou inativo", null);

            // Check if email already exists
            var existingUser = await _context.Users
                .Where(u => u.TenantId == tenantId && u.Email == email && !u.IsDeleted)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                return (false, "Email já está registrado", null);

            // Validate password strength
            if (!IsPasswordStrong(password))
                return (false, "Senha deve ter no mínimo 12 caracteres com maiúsculas, minúsculas, números e símbolos", null);

            var user = new ApplicationUser
            {
                UserId = Guid.NewGuid(),
                TenantId = tenantId,
                Email = email,
                PasswordHash = _passwordService.HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                Role = "User",
                IsTwoFactorEnabled = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _emailService.SendWelcomeEmailAsync(email, firstName);
            _logger.LogInformation($"User {email} registered successfully in tenant {tenantId}");

            return (true, "Usuário registrado com sucesso", user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return (false, "Erro ao registrar usuário", null);
        }
    }

    public async Task<(bool Success, string Message, ApplicationUser? User)> LoginUserAsync(
        string email, string password, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.TenantId == tenantId && u.Email == email && !u.IsDeleted)
                .FirstOrDefaultAsync();

            if (user == null)
                return (false, "Email ou senha inválidos", null);

            if (user.IsBlocked)
                return (false, "Usuário bloqueado pelo gestor", null);

            if (!_passwordService.VerifyPassword(password, user.PasswordHash))
                return (false, "Email ou senha inválidos", null);

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {email} logged in successfully");
            return (true, "Login realizado com sucesso", user);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return (false, "Erro ao fazer login", null);
        }
    }

    public async Task<(string Code, DateTime ExpiresAt)> GenerateTwoFactorCodeAsync(Guid userId)
    {
        try
        {
            var code = _twoFactorService.GenerateCode();
            var expiresAt = _twoFactorService.GetExpirationTime(15);

            var twoFactorLog = new TwoFactorLog
            {
                LogId = Guid.NewGuid(),
                UserId = userId,
                Code = code,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.TwoFactorLogs.Add(twoFactorLog);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                await _emailService.SendTwoFactorCodeAsync(user.Email, code);
            }

            return (code, expiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating 2FA code: {ex.Message}");
            throw;
        }
    }

    public async Task<(bool Success, string Message)> VerifyTwoFactorCodeAsync(Guid userId, string code)
    {
        try
        {
            var twoFactorLog = await _context.TwoFactorLogs
                .Where(tfl => tfl.UserId == userId && tfl.Code == code && !tfl.IsUsed)
                .OrderByDescending(tfl => tfl.CreatedAt)
                .FirstOrDefaultAsync();

            if (twoFactorLog == null)
                return (false, "Código inválido");

            if (_twoFactorService.IsCodeExpired(twoFactorLog.ExpiresAt))
                return (false, "Código expirado");

            twoFactorLog.IsUsed = true;
            await _context.SaveChangesAsync();

            return (true, "Código validado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error verifying 2FA code: {ex.Message}");
            return (false, "Erro ao validar código");
        }
    }

    public async Task<bool> PromoteToGestorAsync(Guid userId, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId && u.TenantId == tenantId && !u.IsDeleted)
                .FirstOrDefaultAsync();

            if (user == null)
                return false;

            user.Role = "Gestor";
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} promoted to Gestor");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error promoting user: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PromoteToAdminAsync(Guid userId, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId && u.TenantId == tenantId && !u.IsDeleted)
                .FirstOrDefaultAsync();

            if (user == null)
                return false;

            user.Role = "Admin";
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} promoted to Admin");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error promoting user: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> BlockUserAsync(Guid userId, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId && u.TenantId == tenantId && !u.IsDeleted)
                .FirstOrDefaultAsync();

            if (user == null)
                return false;

            user.IsBlocked = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} blocked");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error blocking user: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UnblockUserAsync(Guid userId, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId && u.TenantId == tenantId && !u.IsDeleted)
                .FirstOrDefaultAsync();

            if (user == null)
                return false;

            user.IsBlocked = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} unblocked");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unblocking user: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SoftDeleteUserAsync(Guid userId, Guid tenantId)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId && u.TenantId == tenantId)
                .FirstOrDefaultAsync();

            if (user == null)
                return false;

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} soft deleted");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error soft deleting user: {ex.Message}");
            return false;
        }
    }

    private bool IsPasswordStrong(string password)
    {
        if (password.Length < 12)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
