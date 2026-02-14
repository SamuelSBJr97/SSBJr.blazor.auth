# 🚀 Instruções Rápidas - Migrations do Entity Framework

## ⚡ Primeira Vez (Setup Inicial)

### 1️⃣ Verificar Conexão com Banco

Editar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SSBJrWebAuth;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 2️⃣ Criar Migration Inicial

**Option A - Terminal (Recomendado):**
```bash
dotnet ef migrations add InitialCreate
```

**Option B - Visual Studio Package Manager Console:**
```powershell
Add-Migration InitialCreate
```

### 3️⃣ Aplicar ao Banco

**Option A - Terminal:**
```bash
dotnet ef database update
```

**Option B - Visual Studio Package Manager Console:**
```powershell
Update-Database
```

## 📋 Comandos Comuns

### Ver Status das Migrations
```bash
dotnet ef migrations list
```

### Ver SQL que será Executado
```bash
dotnet ef migrations script
```

### Remover Última Migration (Não Aplicada)
```bash
dotnet ef migrations remove
```

### Reverter o Banco para Estado Anterior
```bash
dotnet ef database update NomeDaMigrationAnterior
```

## 📝 Após Modificar Models

1. Adicione/altere propriedades nas classes de modelo
2. Crie nova migration:
```bash
dotnet ef migrations add NomeDescritivo
```

3. Revise o arquivo em `Migrations/`
4. Aplique:
```bash
dotnet ef database update
```

## 🔧 Scripts de Automação

### Windows (PowerShell)
```powershell
# Criar migration
.\scripts\manage-migrations.ps1 -Action create -MigrationName AddPhoneNumber

# Aplicar
.\scripts\manage-migrations.ps1 -Action update

# Listar
.\scripts\manage-migrations.ps1 -Action list

# Remover
.\scripts\manage-migrations.ps1 -Action remove

# Gerar SQL
.\scripts\manage-migrations.ps1 -Action script

# Reverter
.\scripts\manage-migrations.ps1 -Action revert -MigrationName InitialCreate
```

### Linux/Mac (Bash)
```bash
# Criar migration
./scripts/manage-migrations.sh create AddPhoneNumber

# Aplicar
./scripts/manage-migrations.sh update

# Listar
./scripts/manage-migrations.sh list

# Remover
./scripts/manage-migrations.sh remove

# Gerar SQL
./scripts/manage-migrations.sh script

# Reverter
./scripts/manage-migrations.sh revert InitialCreate
```

## ✅ Checklist

- [ ] SQL Server instalado e rodando
- [ ] Connection string configurada
- [ ] EF Core CLI instalado (`dotnet ef` funciona)
- [ ] Migration criada: `dotnet ef migrations add InitialCreate`
- [ ] Banco atualizado: `dotnet ef database update`
- [ ] Tabelas criadas no SQL Server

## 🐛 Problemas Comuns

### "dotnet ef: command not found"
```bash
dotnet tool install --global dotnet-ef
```

### "Could not find a design-time factory"
```bash
# Certifique-se de estar na pasta certa
cd SSBJr.WebAuth
dotnet ef migrations add InitialCreate
```

### "Connection string not found"
Verificar `appsettings.json` tem `DefaultConnection`

### "Cannot open database"
```sql
-- Criar banco manualmente no SQL Server
CREATE DATABASE SSBJrWebAuth;
```

## 📚 Documentação Completa

Ver `MIGRATIONS_GUIDE.md` para guia detalhado com:
- Connection strings para diferentes ambientes
- Troubleshooting avançado
- Migrations em produção
- Seeds e dados iniciais

## 🎯 Próximo Passo

Depois das migrations aplicadas:

1. Criar um tenant de teste:
```bash
dotnet run
# A aplicação vai criar tabelas automaticamente
```

2. Acessar `https://localhost:5001/register` e criar uma conta

3. Fazer login com 2FA

---

**Dúvidas?** Consulte `MIGRATIONS_GUIDE.md`
