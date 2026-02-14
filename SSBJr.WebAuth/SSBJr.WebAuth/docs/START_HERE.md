# INSTRUÇÕES IMEDIATAS - MIGRATIONS

## ⚡ Copie e Cole (Escolha seu SO)

### 🪟 Windows (PowerShell)
```powershell
cd SSBJr.WebAuth
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

### 🐧 Linux / 🍎 Mac (Bash/Zsh)
```bash
cd SSBJr.WebAuth
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

---

## ✅ O Que Acontece

```
1. Cria arquivo de migration (Migrations/InitialCreate.cs)
2. Executa migration no SQL Server
3. Cria todas as 5 tabelas
4. Inicia a aplicação
```

---

## 🌐 Acessar

```
URL: https://localhost:5001/login

Email: admin@teste.com
Senha: AdminPassword123!
Tenant: teste
```

---

## 📚 Se Tiver Dúvidas

Leia em ordem:
1. `MIGRATIONS_SUMMARY.md` - Resumo rápido
2. `MIGRATIONS_QUICK_START.md` - Exemplos práticos
3. `MIGRATIONS_GUIDE.md` - Guia completo

---

## ✨ É ISSO!

Sua aplicação está 100% pronta. Basta executar os 4 comandos acima!

**Boa sorte! 🚀**
