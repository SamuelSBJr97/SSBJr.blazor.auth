#!/bin/bash
# Quick Start Script - SSBJr WebAuth
# Executa todos os passos para iniciar a aplicação

clear

echo ""
echo "╔════════════════════════════════════════════════╗"
echo "║   SSBJr WebAuth - Quick Start                  ║"
echo "║   Autenticação Multitenant Segura              ║"
echo "╚════════════════════════════════════════════════╝"
echo ""

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m'

function write_step() {
    echo -e "\n${CYAN}[$1] $2${NC}"
}

function write_success() {
    echo -e "  ${GREEN}✓ $1${NC}"
}

function write_warning() {
    echo -e "  ${YELLOW}⚠ $1${NC}"
}

function write_error() {
    echo -e "  ${RED}✗ $1${NC}"
}

# Step 1: Verificar Pré-requisitos
write_step "1" "Verificando Pré-requisitos"

if ! command -v dotnet &> /dev/null; then
    write_error "Dotnet SDK não encontrado"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
write_success "Dotnet SDK: $DOTNET_VERSION"

# Step 2: Restaurar dependências
write_step "2" "Restaurando pacotes NuGet"
echo "  Executando: dotnet restore"
dotnet restore

if [ $? -ne 0 ]; then
    write_error "Erro ao restaurar pacotes"
    exit 1
fi
write_success "Pacotes restaurados com sucesso"

# Step 3: Compilar Solução
write_step "3" "Compilando solução"
echo "  Executando: dotnet build"
dotnet build

if [ $? -ne 0 ]; then
    write_error "Erro ao compilar solução"
    exit 1
fi
write_success "Compilação bem-sucedida"

# Step 4: Criar Migration
write_step "4" "Criando Migration Inicial"
echo "  Executando: dotnet ef migrations add InitialCreate"
cd SSBJr.WebAuth
dotnet ef migrations add InitialCreate

if [ $? -ne 0 ]; then
    write_warning "Migration pode já existir, continuando..."
fi
write_success "Migration criada/verificada"

# Step 5: Aplicar Migration
write_step "5" "Aplicando Migrations ao Banco de Dados"
echo "  Executando: dotnet ef database update"
dotnet ef database update

if [ $? -ne 0 ]; then
    write_error "Erro ao atualizar banco de dados"
    exit 1
fi
write_success "Banco de dados atualizado"

# Step 6: Exibir Informações de Teste
echo ""
echo -e "${GREEN}╔════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║   ✅ SETUP CONCLUÍDO COM SUCESSO!             ║${NC}"
echo -e "${GREEN}╚════════════════════════════════════════════════╝${NC}"

echo -e "\n${CYAN}📋 Credenciais de Teste:${NC}"
echo "  ┌─────────────────────────────────────────┐"
echo "  │ Admin                                   │"
echo "  │  Email: admin@teste.com                 │"
echo "  │  Senha: AdminPassword123!               │"
echo "  │  Tenant: teste                          │"
echo "  └─────────────────────────────────────────┘"
echo ""
echo "  ┌─────────────────────────────────────────┐"
echo "  │ Gestor                                  │"
echo "  │  Email: gestor@teste.com                │"
echo "  │  Senha: GestorPassword123!              │"
echo "  │  Tenant: teste                          │"
echo "  └─────────────────────────────────────────┘"
echo ""
echo "  ┌─────────────────────────────────────────┐"
echo "  │ Usuário                                 │"
echo "  │  Email: user@teste.com                  │"
echo "  │  Senha: UserPassword123!                │"
echo "  │  Tenant: teste                          │"
echo "  └─────────────────────────────────────────┘"

echo -e "\n${CYAN}🚀 Para iniciar a aplicação:${NC}"
echo "  dotnet run"

echo -e "\n${CYAN}🌐 Acessar em:${NC}"
echo "  https://localhost:5001"

echo -e "\n${CYAN}📚 Documentação:${NC}"
echo "  - README.md"
echo "  - IMPLEMENTATION_SUMMARY.md"
echo "  - MIGRATIONS_QUICK_START.md"
echo "  - MIGRATIONS_GUIDE.md"

echo -e "\n${GREEN}Pronto! Execute 'dotnet run' para iniciar a aplicação.\n${NC}"
