using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SSBJr.WebAuth.Data;
using SSBJr.WebAuth.Data.Models;

namespace SSBJr.WebAuth.Services;

public interface ITenantService
{
    Task<Tenant?> CreateTenantAsync(string name, string slug);
    Task<Tenant?> GetTenantBySlugAsync(string slug);
    Task<Tenant?> GetTenantByIdAsync(Guid tenantId);
    Task<bool> DeactivateTenantAsync(Guid tenantId);
}

public class TenantService : ITenantService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TenantService> _logger;

    public TenantService(AppDbContext context, ILogger<TenantService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Tenant?> CreateTenantAsync(string name, string slug)
    {
        try
        {
            var existingTenant = await _context.Tenants
                .Where(t => t.Slug == slug)
                .FirstOrDefaultAsync();

            if (existingTenant != null)
            {
                _logger.LogWarning($"Tenant with slug {slug} already exists");
                return null;
            }

            var tenant = new Tenant
            {
                TenantId = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tenant {name} created successfully");
            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating tenant: {ex.Message}");
            return null;
        }
    }

    public async Task<Tenant?> GetTenantBySlugAsync(string slug)
    {
        try
        {
            return await _context.Tenants
                .Where(t => t.Slug == slug && t.IsActive)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting tenant by slug: {ex.Message}");
            return null;
        }
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
    {
        try
        {
            return await _context.Tenants
                .Where(t => t.TenantId == tenantId && t.IsActive)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting tenant: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeactivateTenantAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

            tenant.IsActive = false;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tenant {tenantId} deactivated");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deactivating tenant: {ex.Message}");
            return false;
        }
    }
}
