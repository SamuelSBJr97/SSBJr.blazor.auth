#!/usr/bin/env pwsh
# Quick Start Script - SSBJr WebAuth
# Executa todos os passos para iniciar a aplicação

Write-Host "`n" -ForegroundColor Cyan
Write-Host "╔════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   SSBJr WebAuth - Quick Start                  ║" -ForegroundColor Cyan
Write-Host "║   Autenticação Multitenant Segura              ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Cores
$Success = 'Green'
$Warning = 'Yellow'
$Error = 'Red'
$Info = 'Cyan'

function Write-Step {
    param([string]$Message, [int]$Number)
    Write-Host "`n[$Number] $Message" -ForegroundColor $Info
}

function Write-Success {
    param([string]$Message)
    Write-Host "  ✓ $Message" -ForegroundColor $Success
}

function Write-Warning {
    param([string]$Message)
    Write-Host "  ⚠ $Message" -ForegroundColor $Warning
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "  ✗ $Message" -ForegroundColor $Error
}

# Step 1: Verificar Pré-requisitos
Write-Step "Verificando Pré-requisitos" 1

try {
    $dotnetVersion = dotnet --version
    Write-Success "Dotnet SDK: $dotnetVersion"
} catch {
    Write-Error-Custom "Dotnet SDK não encontrado"
    exit 1
}

try {
    dotnet ef --version | Out-Null
    Write-Success "EF Core CLI instalado"
} catch {
    Write-Warning "EF Core CLI não encontrado. Instalando..."
    dotnet tool install --global dotnet-ef
    Write-Success "EF Core CLI instalado"
}

# Step 2: Verificar SQL Server
Write-Step "Verificando SQL Server" 2
try {
    $connection = "Server=.;Database=master;Trusted_Connection=true;TrustServerCertificate=true;"
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection
    $sqlConnection.ConnectionString = $connection
    $sqlConnection.Open()
    $sqlConnection.Close()
    Write-Success "SQL Server acessível"
} catch {
    Write-Error-Custom "SQL Server não está acessível"
    Write-Host "  Certifique-se de que SQL Server está rodando"
    exit 1
}

# Step 3: Restaurar dependências
Write-Step "Restaurando pacotes NuGet" 3
Write-Host "  Executando: dotnet restore"
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Erro ao restaurar pacotes"
    exit 1
}
Write-Success "Pacotes restaurados com sucesso"

# Step 4: Compilar Solução
Write-Step "Compilando solução" 4
Write-Host "  Executando: dotnet build"
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Erro ao compilar solução"
    exit 1
}
Write-Success "Compilação bem-sucedida"

# Step 5: Criar Migration
Write-Step "Criando Migration Inicial" 5
Write-Host "  Executando: dotnet ef migrations add InitialCreate"
cd SSBJr.WebAuth
dotnet ef migrations add InitialCreate

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Erro ao criar migration"
    Write-Host "  Se a migration já existe, continue normalmente"
}
Write-Success "Migration criada/verificada"

# Step 6: Aplicar Migration
Write-Step "Aplicando Migrations ao Banco de Dados" 6
Write-Host "  Executando: dotnet ef database update"
dotnet ef database update

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Erro ao atualizar banco de dados"
    exit 1
}
Write-Success "Banco de dados atualizado"

# Step 7: Exibir Informações de Teste
Write-Host "`n" -ForegroundColor Green
Write-Host "╔════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   ✅ SETUP CONCLUÍDO COM SUCESSO!             ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Green

Write-Host "`n📋 Credenciais de Teste:" -ForegroundColor $Info
Write-Host "  ┌─────────────────────────────────────────┐"
Write-Host "  │ Admin                                   │"
Write-Host "  │  Email: admin@teste.com                 │"
Write-Host "  │  Senha: AdminPassword123!               │"
Write-Host "  │  Tenant: teste                          │"
Write-Host "  └─────────────────────────────────────────┘"
Write-Host ""
Write-Host "  ┌─────────────────────────────────────────┐"
Write-Host "  │ Gestor                                  │"
Write-Host "  │  Email: gestor@teste.com                │"
Write-Host "  │  Senha: GestorPassword123!              │"
Write-Host "  │  Tenant: teste                          │"
Write-Host "  └─────────────────────────────────────────┘"
Write-Host ""
Write-Host "  ┌─────────────────────────────────────────┐"
Write-Host "  │ Usuário                                 │"
Write-Host "  │  Email: user@teste.com                  │"
Write-Host "  │  Senha: UserPassword123!                │"
Write-Host "  │  Tenant: teste                          │"
Write-Host "  └─────────────────────────────────────────┘"

Write-Host "`n🚀 Para iniciar a aplicação:" -ForegroundColor $Info
Write-Host "  dotnet run"

Write-Host "`n🌐 Acessar em:" -ForegroundColor $Info
Write-Host "  https://localhost:5001"

Write-Host "`n📚 Documentação:" -ForegroundColor $Info
Write-Host "  - README.md"
Write-Host "  - IMPLEMENTATION_SUMMARY.md"
Write-Host "  - MIGRATIONS_QUICK_START.md"
Write-Host "  - MIGRATIONS_GUIDE.md"

Write-Host "`n" -ForegroundColor Green
Write-Host "Pronto! Execute 'dotnet run' para iniciar a aplicação.`n" -ForegroundColor Green
