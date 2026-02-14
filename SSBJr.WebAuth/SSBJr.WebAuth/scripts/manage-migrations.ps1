#!/usr/bin/env pwsh
# Script de automação para Entity Framework Core Migrations
# Uso: .\scripts\manage-migrations.ps1 -Action <action> -MigrationName <name> -Environment <env>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("create", "update", "list", "remove", "script", "revert")]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [string]$MigrationName,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Development", "Production", "Staging")]
    [string]$Environment = "Development"
)

$projectPath = "SSBJr.WebAuth"

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

Write-Header "EF Core Migration Manager"

switch ($Action) {
    "create" {
        if (-not $MigrationName) {
            Write-Error-Custom "Nome da migration é obrigatório para a ação 'create'"
            Write-Host "Uso: .\manage-migrations.ps1 -Action create -MigrationName <nome>"
            exit 1
        }
        
        Write-Host "Criando migration: $MigrationName" -ForegroundColor Yellow
        dotnet ef migrations add $MigrationName --project $projectPath --context AppDbContext
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Migration '$MigrationName' criada com sucesso!"
            Write-Host "`nNa pasta Migrations/$MigrationName.cs você pode revisar as alterações."
        } else {
            Write-Error-Custom "Erro ao criar a migration"
            exit 1
        }
    }
    
    "update" {
        Write-Host "Aplicando migrations ao banco ($Environment)..." -ForegroundColor Yellow
        dotnet ef database update --project $projectPath --environment $Environment
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Banco de dados atualizado com sucesso!"
        } else {
            Write-Error-Custom "Erro ao atualizar o banco de dados"
            exit 1
        }
    }
    
    "list" {
        Write-Header "Migrations Disponíveis"
        dotnet ef migrations list --project $projectPath
    }
    
    "remove" {
        Write-Host "Removendo a última migration..." -ForegroundColor Yellow
        
        $confirm = Read-Host "Tem certeza? [S/n]"
        if ($confirm -eq "S" -or $confirm -eq "s" -or $confirm -eq "") {
            dotnet ef migrations remove --project $projectPath
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Migration removida com sucesso!"
            } else {
                Write-Error-Custom "Erro ao remover a migration"
                exit 1
            }
        } else {
            Write-Host "Operação cancelada."
        }
    }
    
    "script" {
        Write-Host "Gerando script SQL..." -ForegroundColor Yellow
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $outputFile = "migrations_$timestamp.sql"
        
        dotnet ef migrations script --output $outputFile --project $projectPath
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Script SQL gerado: $outputFile"
            Write-Host "`nVocê pode executar este script diretamente no SQL Server Management Studio."
        } else {
            Write-Error-Custom "Erro ao gerar o script SQL"
            exit 1
        }
    }
    
    "revert" {
        if (-not $MigrationName) {
            Write-Error-Custom "Nome da migration é obrigatório para reverter"
            Write-Host "Uso: .\manage-migrations.ps1 -Action revert -MigrationName <nome>"
            exit 1
        }
        
        Write-Host "Revertendo para migration: $MigrationName" -ForegroundColor Yellow
        
        $confirm = Read-Host "Tem certeza? Isso vai descartar dados! [S/n]"
        if ($confirm -eq "S" -or $confirm -eq "s") {
            dotnet ef database update $MigrationName --project $projectPath --environment $Environment
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Revertido para: $MigrationName"
            } else {
                Write-Error-Custom "Erro ao reverter"
                exit 1
            }
        } else {
            Write-Host "Operação cancelada."
        }
    }
}

Write-Host "`n"
