using Microsoft.EntityFrameworkCore;
using SSBJr.WebAuth.Data;
using SSBJr.WebAuth.Data.Models;
using SSBJr.WebAuth.Services;

namespace SSBJr.WebAuth.Extensions;

public static class SeedExtensions
{
    public static async Task SeedDataAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

            // Seed tenants
            await SeedTenantsAsync(context, tenantService);

            // Seed users
            await SeedUsersAsync(context, authService, tenantService);
        }
    }

    private static async Task SeedTenantsAsync(AppDbContext context, ITenantService tenantService)
    {
        // Verificar se já existe um tenant
        var existingTenant = await context.Tenants
            .Where(t => t.Slug == "teste")
            .FirstOrDefaultAsync();

        if (existingTenant != null)
        {
            Console.WriteLine("✓ Tenant 'teste' já existe. Pulando seed.");
            return;
        }

        Console.WriteLine("→ Criando tenant de teste...");

        var tenant = await tenantService.CreateTenantAsync("Empresa Teste", "teste");

        if (tenant != null)
        {
            Console.WriteLine("✓ Tenant 'teste' criado com sucesso!");
        }
    }

    private static async Task SeedUsersAsync(AppDbContext context, IAuthenticationService authService, ITenantService tenantService)
    {
        var tenant = await tenantService.GetTenantBySlugAsync("teste");
        if (tenant == null)
        {
            Console.WriteLine("✗ Erro: Tenant não encontrado. Pulando seed de usuários.");
            return;
        }

        // Verificar se já existe admin
        var existingAdmin = await context.Users
            .Where(u => u.Email == "admin@teste.com" && u.TenantId == tenant.TenantId)
            .FirstOrDefaultAsync();

        if (existingAdmin != null)
        {
            Console.WriteLine("✓ Usuário admin já existe. Pulando seed.");
            return;
        }

        Console.WriteLine("→ Criando usuários de teste...");

        // Criar Admin
        var adminResult = await authService.RegisterUserAsync(
            "admin@teste.com",
            "AdminPassword123!",
            tenant.TenantId,
            "Admin",
            "Sistema"
        );

        if (adminResult.Success && adminResult.User != null)
        {
            await authService.PromoteToAdminAsync(adminResult.User.UserId, tenant.TenantId);
            Console.WriteLine("✓ Admin criado: admin@teste.com / AdminPassword123!");
        }

        // Criar Gestor
        var gestorResult = await authService.RegisterUserAsync(
            "gestor@teste.com",
            "GestorPassword123!",
            tenant.TenantId,
            "João",
            "Gestor"
        );

        if (gestorResult.Success && gestorResult.User != null)
        {
            await authService.PromoteToGestorAsync(gestorResult.User.UserId, tenant.TenantId);
            Console.WriteLine("✓ Gestor criado: gestor@teste.com / GestorPassword123!");
        }

        // Criar Usuário Comum
        var userResult = await authService.RegisterUserAsync(
            "user@teste.com",
            "UserPassword123!",
            tenant.TenantId,
            "Maria",
            "Usuária"
        );

        if (userResult.Success)
        {
            Console.WriteLine("✓ Usuário criado: user@teste.com / UserPassword123!");
        }

        Console.WriteLine("\n✅ Seed de dados concluído!");
        Console.WriteLine("\nCredenciais de teste:");
        Console.WriteLine("  Email: admin@teste.com    | Senha: AdminPassword123!");
        Console.WriteLine("  Email: gestor@teste.com   | Senha: GestorPassword123!");
        Console.WriteLine("  Email: user@teste.com     | Senha: UserPassword123!");
        Console.WriteLine("  Tenant: teste");
    }
}
