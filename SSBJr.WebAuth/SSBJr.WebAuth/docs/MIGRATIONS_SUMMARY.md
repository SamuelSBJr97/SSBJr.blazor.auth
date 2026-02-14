# 🗄️ Instruções de Migrations - Resumo Executivo

## ⚡ Quick Start (Copie e Cole)

### Windows (PowerShell):
```powershell
cd SSBJr.WebAuth

# Criar migration
dotnet ef migrations add InitialCreate

# Aplicar ao banco
dotnet ef database update

# Ou executar o script automático
.\quick-start.ps1
```

### Linux/Mac (Bash):
```bash
cd SSBJr.WebAuth

# Criar migration
dotnet ef migrations add InitialCreate

# Aplicar ao banco
dotnet ef database update

# Ou executar o script automático
./quick-start.sh
```

---

## 📋 O que Acontece Quando você Roda

### 1. `dotnet ef migrations add InitialCreate`

✅ Cria arquivo em `Migrations/20240xxx_InitialCreate.cs` com:
- Criar tabela `Tenants`
- Criar tabela `Users`
- Criar tabela `TwoFactorLogs`
- Criar tabela `UserObservations`
- Criar tabela `AuditLogs`
- Criar índices e relacionamentos

### 2. `dotnet ef database update`

✅ Executa no SQL Server:
- Cria o banco `SSBJrWebAuth` (se não existir)
- Cria todas as tabelas
- Cria índices
- Cria relacionamentos
- Cria constraints

### 3. Seed de Dados (Automático)

✅ Quando aplicação inicia em Development:
- Cria Tenant "teste"
- Cria Admin: admin@teste.com / AdminPassword123!
- Cria Gestor: gestor@teste.com / GestorPassword123!
- Cria User: user@teste.com / UserPassword123!

---

## ✅ Verificar se Funcionou

### SQL Server Management Studio

```sql
-- Conectar ao servidor
-- Verificar se existe banco "SSBJrWebAuth"
-- Expandir Databases > SSBJrWebAuth > Tables

-- Deve existir:
-- ✓ dbo.Tenants
-- ✓ dbo.Users  
-- ✓ dbo.TwoFactorLogs
-- ✓ dbo.UserObservations
-- ✓ dbo.AuditLogs
```

### Contar registros

```sql
SELECT COUNT(*) as TotalTenants FROM [dbo].[Tenants];
SELECT COUNT(*) as TotalUsers FROM [dbo].[Users];
```

---

## 🔄 Após Modificar Models

Se você modificar uma classe em `Data/Models/`:

```bash
# 1. Criar nova migration
dotnet ef migrations add DescricaoDaMudanca

# Exemplos de nomes descritivos:
# - AddPhoneNumberToUser
# - CreateObservationsTable
# - AddIndexToEmail

# 2. Revisar arquivo criado em Migrations/

# 3. Aplicar ao banco
dotnet ef database update
```

---

## 🆘 Problemas Comuns e Soluções

### ❌ "Could not find a design-time factory"

```bash
# Solução: Estar na pasta correta
cd SSBJr.WebAuth
dotnet ef migrations add InitialCreate
```

### ❌ "Cannot open database 'SSBJrWebAuth'"

```bash
# SQL Server precisa existir e estar acessível
# Testar conexão:
sqlcmd -S . -Q "SELECT @@VERSION"

# Se falhar, SQL Server pode não estar rodando:
# Services > SQL Server (MSSQLSERVER)
```

### ❌ "Migration already exists"

```bash
# Se tentou rodar migrations add novamente:
dotnet ef migrations remove  # Remove a última
# ou simplesmente:
dotnet ef database update    # Apenas aplica
```

### ❌ "Connection string not found"

```bash
# Verificar appsettings.json:
# Deve ter: "DefaultConnection": "Server=...;Database=..."
```

---

## 📊 Estrutura das Tabelas Criadas

### Tenants
```sql
- TenantId (UNIQUEIDENTIFIER, PK)
- Name (NVARCHAR(255))
- Slug (NVARCHAR(100), UNIQUE)
- IsActive (BIT)
- CreatedAt (DATETIME2)
- UpdatedAt (DATETIME2, nullable)
```

### Users
```sql
- UserId (UNIQUEIDENTIFIER, PK)
- TenantId (UNIQUEIDENTIFIER, FK)
- Email (NVARCHAR(255))
- PasswordHash (NVARCHAR(255))
- FirstName, LastName, PhoneNumber (nullable)
- ProfessionalTitle, Department (nullable)
- Address, City, State, ZipCode (nullable)
- Role (NVARCHAR(50))
- IsTwoFactorEnabled, IsBlocked, IsDeleted (BIT)
- CreatedAt, UpdatedAt, LastLoginAt (DATETIME2)
```

### TwoFactorLogs
```sql
- LogId (UNIQUEIDENTIFIER, PK)
- UserId (UNIQUEIDENTIFIER, FK)
- Code (NVARCHAR(6))
- ExpiresAt (DATETIME2)
- IsUsed (BIT)
- CreatedAt (DATETIME2)
```

### UserObservations
```sql
- ObservationId (UNIQUEIDENTIFIER, PK)
- UserId (UNIQUEIDENTIFIER, FK)
- GestorId (UNIQUEIDENTIFIER, FK)
- Observation (NVARCHAR(2000))
- CreatedAt, UpdatedAt (DATETIME2)
```

### AuditLogs
```sql
- AuditId (UNIQUEIDENTIFIER, PK)
- UserId (UNIQUEIDENTIFIER, FK)
- TenantId (UNIQUEIDENTIFIER, FK, nullable)
- Action (NVARCHAR(100))
- Description (NVARCHAR(500), nullable)
- IpAddress (NVARCHAR(45), nullable)
- UserAgent (NVARCHAR(500), nullable)
- CreatedAt (DATETIME2)
```

---

## 🎯 Próximo Passo

```bash
# 1. Executar migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# 2. Iniciar aplicação
dotnet run

# 3. Abrir browser
https://localhost:5001/login

# 4. Logar com
# Email: admin@teste.com
# Senha: AdminPassword123!
# Tenant: teste
```

---

## 📚 Referências

- `MIGRATIONS_GUIDE.md` - Guia completo e detalhado
- `MIGRATIONS_QUICK_START.md` - Exemplos práticos
- `POST_SETUP_GUIDE.md` - Próximos passos

---

**🎉 Pronto! Suas migrations estão configuradas e prontas para rodar!**
