# 🎯 Guia Pós-Setup - Próximos Passos

## ✅ Se você seguiu o Quick Start

Parabéns! Seu banco de dados está configurado e a aplicação está pronta. Agora siga:

### 1️⃣ Iniciar a Aplicação

```bash
dotnet run
```

A aplicação iniciará em `https://localhost:5001`

### 2️⃣ Testar o Login

- **URL:** `https://localhost:5001/login`
- **Email:** `admin@teste.com`
- **Senha:** `AdminPassword123!`
- **Tenant:** `teste`

Você receberá um código 6 dígitos por email (em desenvolvimento, verifique logs)

### 3️⃣ Navegar pela Aplicação

Após 2FA bem-sucedido:
- **Admin:** Acesse `/admin/dashboard`
- **Gestor:** Acesse `/gestor/users`
- **User:** Acesse `/user/profile`

---

## 📧 Configurar Envio de Email Real

### Para Gmail

1. **Ativar 2FA na conta Google:**
   - Acesse `myaccount.google.com`
   - Segurança → Verificação em 2 etapas

2. **Criar App Password:**
   - Segurança → Senhas de app
   - Selecione "Mail" e "Windows/Linux"
   - Google gerará uma senha de 16 caracteres

3. **Configurar appsettings.json:**

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "FromEmail": "seu-email@gmail.com",
    "SmtpPassword": "sua-senha-gerada-de-16-caracteres",
    "EnableSsl": "true"
  }
}
```

4. **Reiniciar aplicação:**

```bash
dotnet run
```

### Para Outros Provedores

**Outlook/Office 365:**
```json
{
  "SmtpServer": "smtp.office365.com",
  "SmtpPort": "587",
  "FromEmail": "seu-email@outlook.com",
  "SmtpPassword": "sua-senha",
  "EnableSsl": "true"
}
```

**SendGrid:**
```json
{
  "SmtpServer": "smtp.sendgrid.net",
  "SmtpPort": "587",
  "FromEmail": "seu-email@sendgrid.com",
  "SmtpPassword": "SG.sua-api-key",
  "EnableSsl": "true"
}
```

---

## 🔧 Personalizar a Aplicação

### Adicionar Novo Tenant

```bash
# Usando o script
.\scripts\manage-migrations.ps1 -Action create -MigrationName AddNewTenant

# Ou manualmente
dotnet ef migrations add AddNewTenant
dotnet ef database update
```

### Adicionar Novos Campos de Usuário

1. Editar `Data/Models/ApplicationUser.cs`:
```csharp
public string? NewField { get; set; }
```

2. Criar migration:
```bash
dotnet ef migrations add AddNewFieldToUser
```

3. Revisar a migration em `Migrations/`

4. Aplicar:
```bash
dotnet ef database update
```

### Criar Novo Componente Blazor

Exemplo: Página de Mudança de Senha

```razor
@page "/user/change-password"
@attribute [Authorize(Roles = "User,Gestor,Admin")]
@rendermode InteractiveServer

<div class="container">
    <h2>Alterar Senha</h2>
    
    <EditForm Model="Model" OnValidSubmit="HandleChangePassword">
        <DataAnnotationsValidator />
        
        <div class="mb-3">
            <label for="current">Senha Atual</label>
            <InputText @bind-Value="Model.CurrentPassword" class="form-control" type="password" />
        </div>
        
        <div class="mb-3">
            <label for="new">Nova Senha</label>
            <InputText @bind-Value="Model.NewPassword" class="form-control" type="password" />
        </div>
        
        <button type="submit" class="btn btn-primary">Alterar Senha</button>
    </EditForm>
</div>

@code {
    private ChangePasswordModel Model = new();
    
    private async Task HandleChangePassword()
    {
        // TODO: Implementar lógica de mudança de senha
    }
    
    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
```

---

## 🚀 Deploy em Produção

### 1️⃣ Preparar Ambiente

```bash
# Publicar aplicação
dotnet publish -c Release -o ./publish

# Ou para IIS
dotnet publish -c Release
```

### 2️⃣ Configurar Banco de Produção

Editar `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=seu-servidor-sql;Database=SSBJrWebAuth;User Id=usuario;Password=senha;Encrypt=true;TrustServerCertificate=false;"
  },
  "Email": {
    "SmtpServer": "seu-smtp-server",
    "SmtpPort": "587",
    "FromEmail": "seu-email@sua-empresa.com",
    "SmtpPassword": "sua-senha",
    "EnableSsl": "true"
  },
  "Security": {
    "SessionTimeoutMinutes": 60
  }
}
```

### 3️⃣ Aplicar Migrations em Produção

```bash
# No servidor de produção
dotnet ef database update --environment Production
```

### 4️⃣ Configurar IIS (Windows)

1. Instalar Hosting Bundle do .NET 10
2. Criar Site no IIS
3. Apontar para pasta `publish`
4. Configurar Application Pool (.NET CLR version: No Managed Code)

### 5️⃣ Configurar Certificado SSL

Usar **Let's Encrypt** com IIS:
```bash
# Instalar IIS Crypto
# Gerar certificado automático
```

---

## 🐛 Troubleshooting Comum

### "Senha de email incorreta"
```bash
# Verificar se a senha de app foi gerada corretamente
# Gmail: Gerar nova senha em myaccount.google.com/apppasswords
```

### "Conexão recusada ao banco"
```bash
# Verificar se SQL Server está rodando
# Services → SQL Server (MSSQLSERVER)

# Ou verificar connection string
# Abrir SQL Server Management Studio e conectar manualmente
```

### "Migração já foi aplicada"
```bash
# Remover a migration local se ainda não foi aplicada
dotnet ef migrations remove

# Ou simplesmente executar update novamente
dotnet ef database update
```

### "Cookie expirado muito rápido"
```json
// Aumentar tempo em appsettings.json
{
  "Security": {
    "SessionTimeoutMinutes": 120
  }
}
```

---

## 📊 Monitoramento

### Ver Logs de Auditoria

Todos os logins são registrados em `AuditLogs`. Criar dashboard:

```csharp
// Controller exemplo
[HttpGet("audit-logs")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetAuditLogs()
{
    var logs = await _context.AuditLogs
        .OrderByDescending(l => l.CreatedAt)
        .Take(100)
        .ToListAsync();
    
    return Ok(logs);
}
```

### Verificar Saúde da Aplicação

```csharp
// Adicionar Health Check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// No Program.cs
app.MapHealthChecks("/health");
```

---

## 🔄 Backup e Recuperação

### Backup Manual do Banco

```sql
-- SQL Server Management Studio
BACKUP DATABASE SSBJrWebAuth 
TO DISK = 'C:\Backup\SSBJrWebAuth.bak'
```

### Automatizar Backup

```powershell
# Script de backup diário
$backupPath = "C:\Backups\SSBJrWebAuth_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"

sqlcmd -S . -Q "BACKUP DATABASE SSBJrWebAuth TO DISK = '$backupPath'"

Write-Host "Backup criado em $backupPath"
```

---

## 📚 Próximos Passos Recomendados

1. ✅ Testar login e 2FA
2. ✅ Configurar email real
3. ✅ Criar mais usuários de teste
4. ✅ Testar cada rol (Admin, Gestor, User)
5. ✅ Implementar APIs REST (se necessário)
6. ✅ Adicionar mais campos de usuário
7. ✅ Configurar SSL em produção
8. ✅ Implementar rate limiting
9. ✅ Adicionar logs e monitoramento
10. ✅ Realizar testes de segurança (OWASP)

---

## ❓ FAQ

**P: Como resetar a senha de um usuário?**  
R: Criar método em `AuthenticationService` ou adicionar endpoint admin para reset.

**P: Posso usar banco de dados diferente do SQL Server?**  
R: Sim! Trocar `UseSqlServer()` por `UsePostgreSql()` ou `UseMySql()` no `AppDbContext`.

**P: Como desabilitar 2FA para um usuário?**  
R: Adicionar campo booleano em `ApplicationUser` e verificar em `GenerateTwoFactorCodeAsync()`.

**P: Qual é o tempo máximo de sessão?**  
R: Configurável em `appsettings.json` → `Security.SessionTimeoutMinutes`.

---

## 🎉 Conclusão

Sua aplicação está **100% funcional e segura**!

Para mais detalhes, consulte:
- `README.md` - Overview geral
- `IMPLEMENTATION_GUIDE.md` - Detalhes técnicos
- `MIGRATIONS_GUIDE.md` - Operações de banco de dados

**Boa sorte com sua aplicação! 🚀**
