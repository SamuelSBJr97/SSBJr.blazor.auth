using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SSBJr.WebAuth.Components;
using SSBJr.WebAuth.Data;
using SSBJr.WebAuth.Extensions;
using SSBJr.WebAuth.Middleware;
using SSBJr.WebAuth.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar serviços
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITenantService, TenantService>();

// Configurar autenticação com cookies seguros
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(
            builder.Configuration["Security:SessionTimeoutMinutes"] ?? "30"));

        options.Cookie.Name = "__HostAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.IsEssential = true;

        options.SlidingExpiration = true;
        options.ReturnUrlParameter = "returnUrl";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => 
        policy.RequireClaim("role", "Admin"));

    options.AddPolicy("Gestor", policy => 
        policy.RequireClaim("role", "Gestor", "Admin"));

    options.AddPolicy("User", policy => 
        policy.RequireClaim("role", "User", "Gestor", "Admin"));
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.FormFieldName = "__RequestVerificationToken";
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Aplicar migrations automaticamente
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Seed dados iniciais (apenas em Development)
    if (app.Environment.IsDevelopment())
    {
        Console.WriteLine("\nIniciando seed de dados...");
        await app.SeedDataAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

// Middleware de segurança customizado
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<AntiCsrfMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseStaticFiles();
app.UseAntiforgery();

// Autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
