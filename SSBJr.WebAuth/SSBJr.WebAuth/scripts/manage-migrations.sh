#!/bin/bash
# Script de automação para Entity Framework Core Migrations
# Uso: ./scripts/manage-migrations.sh create AddPhoneNumberToUser
#      ./scripts/manage-migrations.sh update
#      ./scripts/manage-migrations.sh list

set -e

PROJECT_PATH="SSBJr.WebAuth"

# Cores para output
CYAN='\033[0;36m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Função para imprimir header
print_header() {
    echo -e "${CYAN}"
    echo "========================================"
    echo "$1"
    echo "========================================"
    echo -e "${NC}"
}

# Função para imprimir sucesso
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# Função para imprimir erro
print_error() {
    echo -e "${RED}✗ $1${NC}"
}

# Função para imprimir aviso
print_warning() {
    echo -e "${YELLOW}→ $1${NC}"
}

# Verificar se action foi fornecida
if [ $# -lt 1 ]; then
    print_header "EF Core Migration Manager"
    echo "Ações disponíveis:"
    echo "  create <name>    - Criar nova migration"
    echo "  update           - Aplicar migrations ao banco"
    echo "  list             - Listar todas as migrations"
    echo "  remove           - Remover última migration"
    echo "  script           - Gerar script SQL"
    echo "  revert <name>    - Reverter para migration específica"
    echo ""
    echo "Exemplos:"
    echo "  ./scripts/manage-migrations.sh create AddPhoneNumberToUser"
    echo "  ./scripts/manage-migrations.sh update"
    echo "  ./scripts/manage-migrations.sh list"
    exit 1
fi

ACTION=$1
MIGRATION_NAME=$2
ENVIRONMENT=${3:-Development}

print_header "EF Core Migration Manager"

case $ACTION in
    create)
        if [ -z "$MIGRATION_NAME" ]; then
            print_error "Nome da migration é obrigatório"
            echo "Uso: ./manage-migrations.sh create <nome>"
            exit 1
        fi
        
        print_warning "Criando migration: $MIGRATION_NAME"
        if dotnet ef migrations add "$MIGRATION_NAME" --project "$PROJECT_PATH" --context AppDbContext; then
            print_success "Migration '$MIGRATION_NAME' criada com sucesso!"
            echo ""
            echo "Na pasta Migrations/ você pode revisar as alterações."
        else
            print_error "Erro ao criar a migration"
            exit 1
        fi
        ;;
    
    update)
        print_warning "Aplicando migrations ao banco ($ENVIRONMENT)..."
        if dotnet ef database update --project "$PROJECT_PATH" --environment "$ENVIRONMENT"; then
            print_success "Banco de dados atualizado com sucesso!"
        else
            print_error "Erro ao atualizar o banco de dados"
            exit 1
        fi
        ;;
    
    list)
        print_header "Migrations Disponíveis"
        dotnet ef migrations list --project "$PROJECT_PATH"
        ;;
    
    remove)
        print_warning "Removendo a última migration..."
        read -p "Tem certeza? [s/N] " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Ss]$ ]]; then
            if dotnet ef migrations remove --project "$PROJECT_PATH"; then
                print_success "Migration removida com sucesso!"
            else
                print_error "Erro ao remover a migration"
                exit 1
            fi
        else
            echo "Operação cancelada."
        fi
        ;;
    
    script)
        print_warning "Gerando script SQL..."
        TIMESTAMP=$(date +%Y%m%d_%H%M%S)
        OUTPUT_FILE="migrations_$TIMESTAMP.sql"
        
        if dotnet ef migrations script --output "$OUTPUT_FILE" --project "$PROJECT_PATH"; then
            print_success "Script SQL gerado: $OUTPUT_FILE"
            echo ""
            echo "Você pode executar este script diretamente no SQL Server Management Studio."
        else
            print_error "Erro ao gerar o script SQL"
            exit 1
        fi
        ;;
    
    revert)
        if [ -z "$MIGRATION_NAME" ]; then
            print_error "Nome da migration é obrigatório para reverter"
            echo "Uso: ./manage-migrations.sh revert <nome>"
            exit 1
        fi
        
        print_warning "Revertendo para migration: $MIGRATION_NAME"
        read -p "Tem certeza? Isso vai descartar dados! [s/N] " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Ss]$ ]]; then
            if dotnet ef database update "$MIGRATION_NAME" --project "$PROJECT_PATH" --environment "$ENVIRONMENT"; then
                print_success "Revertido para: $MIGRATION_NAME"
            else
                print_error "Erro ao reverter"
                exit 1
            fi
        else
            echo "Operação cancelada."
        fi
        ;;
    
    *)
        print_error "Ação desconhecida: $ACTION"
        echo "Ações disponíveis: create, update, list, remove, script, revert"
        exit 1
        ;;
esac

echo ""
