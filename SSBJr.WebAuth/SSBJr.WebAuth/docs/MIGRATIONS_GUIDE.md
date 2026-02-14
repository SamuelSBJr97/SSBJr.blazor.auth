# Guia de Migrations - Entity Framework Core

Este guia explica como executar as migrations do Entity Framework Core para criar e atualizar o banco de dados.

## Pré-requisitos

- .NET 10 SDK instalado
- SQL Server acessível
- Visual Studio, VS Code ou PowerShell
- Connection string configurada em `appsettings.json`

## Configuração Inicial

### 1. Verificar/Configurar a Connection String

Editar `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SSBJrWebAuth;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Exemplos de connection strings:**

**SQL Server Local (Windows Auth):**
```
Server=.;Database=SSBJrWebAuth;Trusted_Connection=true;TrustServerCertificate=true;
```

**SQL Server Local (SQL Auth):**
```
Server=localhost;Database=SSBJrWebAuth;User Id=sa;Password=YourPassword;TrustServerCertificate=true;
```

**SQL Server Remoto:**
```
Server=seu-servidor.database.windows.net;Database=SSBJrWebAuth;User Id=usuario;Password=senha;Encrypt=true;TrustServerCertificate=false;
```

**Development (SQLite - opcional):**
```
Data Source=app.db;
```

### 2. Instalar o EF Core CLI (se necessário)

```bash
dotnet tool install --global dotnet-ef
```

Ou atualizar se já estiver instalado:

```bash
dotnet tool update --global dotnet-ef
```

## Criando a Migration Inicial

### Opção 1: Usando Visual Studio Package Manager Console

1. Abrir **Package Manager Console**
   - Tools → NuGet Package Manager → Package Manager Console

2. Executar o comando:

```powershell
Add-Migration InitialCreate
```

3. Atualizar o banco:

```powershell
Update-Database
```

### Opção 2: Usando .NET CLI (Recomendado)

Abrir o terminal na raiz do projeto e executar:

```bash
# Criar a migration
dotnet ef migrations add InitialCreate

# Aplicar a migration ao banco
dotnet ef database update
```

## Verificando o Status

### Ver todas as migrations criadas

```bash
dotnet ef migrations list
```

Saída esperada:
```
Build started...
Build succeeded.

20240115120000_InitialCreate (Pending)
```

### Ver o SQL que será executado

Antes de aplicar a migration, você pode visualizar o SQL:

```bash
dotnet ef migrations script InitialCreate
```

Ou para uma range de migrations:

```bash
dotnet ef migrations script 20240115120000_InitialCreate 20240115130000_NextMigration
```

## Aplicando as Migrations

### Aplicar todas as migrations pendentes

```bash
dotnet ef database update
```

### Aplicar até uma migration específica

```bash
dotnet ef database update InitialCreate
```

### Reverter para uma migration anterior

```bash
dotnet ef database update PreviousMigrationName
```

Exemplo (remover a última migration):
```bash
dotnet ef database update 0
```

## Criando Novas Migrations (Após Modificações no Modelo)

Sempre que você modificar as classes de modelo de dados:

1. Faça as alterações nas classes (ex: adicionar propriedades)

2. Crie uma nova migration:

```bash
dotnet ef migrations add NomeDescritivo
```

Exemplos:
```bash
dotnet ef migrations add AddPhoneNumberToUser
dotnet ef migrations add CreateUserObservationsTable
dotnet ef migrations add AddIndexToEmail
```

3. Revise o arquivo de migration criado em `Migrations/`:

```csharp
public partial class AddPhoneNumberToUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Seu código aqui
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverter as mudanças aqui
    }
}
```

4. Aplique a migration:

```bash
dotnet ef database update
```

## Removendo Migrations

### Remover a última migration (não aplicada)

```bash
dotnet ef migrations remove
```

⚠️ **Aviso:** Só faça isso se a migration ainda não foi aplicada ao banco de dados.

### Remover uma migration aplicada (Cuidado!)

```bash
# Reverter o banco para a migration anterior
dotnet ef database update PreviousMigrationName

# Remover a migration
dotnet ef migrations remove
```

## Troubleshooting

### Erro: "Could not find a design-time factory for DbContext"

**Solução:**
```bash
# Certifique-se de estar na pasta raiz do projeto
cd SSBJr.WebAuth

# Se isso não funcionar, especifique o projeto
dotnet ef migrations add InitialCreate --project SSBJr.WebAuth
```

### Erro: "Connection string not found"

**Solução:**
1. Verificar se `appsettings.json` tem a connection string
2. Especificar o environment:

```bash
# Para Development
dotnet ef database update --environment Development

# Para Production
dotnet ef database update --environment Production
```

### Erro: "Cannot open database"

**Solução:**
1. Verificar se o SQL Server está rodando
2. Verificar a connection string
3. Criar o banco manualmente antes (SQL Server requer isso às vezes):

```sql
CREATE DATABASE SSBJrWebAuth;
```

### Erro: "An error occurred while accessing the Microsoft.Extensions.Logging..."

**Solução:**
```bash
# Limpar e reconstruir
dotnet clean
dotnet build
dotnet ef database update
```

## Migrations em Diferentes Ambientes

### Development

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SSBJrWebAuth_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

```bash
dotnet ef database update --environment Development
```

### Production

Usar `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=seu-servidor;Database=SSBJrWebAuth;User Id=usuario;Password=senha;Encrypt=true;"
  }
}
```

```bash
dotnet ef database update --environment Production
```

## Aplicar Migrations Automaticamente na Startup

O código no `Program.cs` já faz isso:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}
```

Isso significa que as migrations serão aplicadas automaticamente quando a aplicação iniciar.

Para desabilitar:
```csharp
// Remova ou comente as linhas acima se não quiser auto-migrate
```

## Script SQL Direto

Se preferir executar o SQL diretamente:

### Gerar o script SQL completo

```bash
dotnet ef migrations script --output migrations.sql
```

### Gerar script de uma range específica

```bash
dotnet ef migrations script InitialCreate AddPhoneNumberToUser --output update.sql
```

Depois execute o `.sql` diretamente no SQL Server Management Studio.

## Checklist de Migrations

- [ ] Verificar `appsettings.json` com connection string correta
- [ ] Rodar `dotnet ef migrations add InitialCreate`
- [ ] Revisar o arquivo de migration em `Migrations/`
- [ ] Rodar `dotnet ef database update`
- [ ] Verificar no SQL Server que as tabelas foram criadas
- [ ] Testar a aplicação

## Próximas Etapas

Após as migrations criadas:

1. **Criar um Tenant Seed:**
```csharp
// Adicionar ao Program.cs após migrate
using (var scope = app.Services.CreateScope())
{
    var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
    await tenantService.CreateTenantAsync("Teste", "teste");
}
```

2. **Criar um Usuário Admin:**
```csharp
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
    var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
    var tenant = await tenantService.GetTenantBySlugAsync("teste");
    
    var result = await authService.RegisterUserAsync(
        "admin@test.com",
        "AdminPass123!",
        tenant!.TenantId,
        "Admin",
        "User"
    );
    
    if (result.Success)
    {
        await authService.PromoteToAdminAsync(result.User!.UserId, tenant.TenantId);
    }
}
```

## Referências

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core CLI Commands](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [SQL Server Connection Strings](https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlclient.sqlconnection.connectionstring)

---

**Última atualização:** 2024
**Framework:** .NET 10 + Entity Framework Core 8
**Database:** SQL Server
