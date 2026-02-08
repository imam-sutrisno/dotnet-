# Security Best Practices

## Connection String Security

### ❌ Never Do This
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=myserver;Password=MyRealPassword123!;"
  }
}
```

### ✅ Development - Use User Secrets
```bash
# Initialize user secrets
cd /path/to/project
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=ProductDB;User=root;Password=your_dev_password"

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
```

### ✅ Production - Use Environment Variables
```bash
# Linux/Mac
export ConnectionStrings__DefaultConnection="Server=prod-server;Port=3306;Database=ProductDB;User=appuser;Password=secure_prod_password"

# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=prod-server;Port=3306;Database=ProductDB;User=appuser;Password=secure_prod_password"

# Windows CMD
set ConnectionStrings__DefaultConnection=Server=prod-server;Port=3306;Database=ProductDB;User=appuser;Password=secure_prod_password
```

### ✅ Azure - Use App Configuration or Key Vault
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Configuration Hierarchy

ASP.NET Core reads configuration in this order (later overrides earlier):
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments

## Best Practices

### 1. Separate Configuration by Environment
```
appsettings.json              # Base configuration, no secrets
appsettings.Development.json  # Development overrides (gitignored)
appsettings.Production.json   # Production overrides (if needed)
```

### 2. Use .gitignore
```gitignore
# Sensitive configuration files
appsettings.Development.json
appsettings.*.json
!appsettings.json
```

### 3. Placeholder Values in appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=ProductDB;User=root;Password=;"
  }
}
```

### 4. Document Required Configuration
```json
// appsettings.json
{
  "ConnectionStrings": {
    // Update with your MySQL credentials
    // Development: Use user secrets (dotnet user-secrets set ...)
    // Production: Use environment variables
    "DefaultConnection": "Server=localhost;Port=3306;Database=ProductDB;User=root;Password=;"
  }
}
```

## SQL Injection Prevention

### ❌ String Concatenation (Vulnerable)
```csharp
var sql = $"SELECT * FROM Products WHERE Name = '{productName}'";
var products = await connection.QueryAsync<Product>(sql);
```

### ✅ Parameterized Queries (Safe)
```csharp
var sql = "SELECT * FROM Products WHERE Name = @Name";
var products = await connection.QueryAsync<Product>(sql, new { Name = productName });
```

### ✅ Always Use Parameters
```csharp
// Good
var sql = "SELECT * FROM Products WHERE Category = @Category AND Price > @MinPrice";
var products = await connection.QueryAsync<Product>(
    sql, 
    new { Category = category, MinPrice = minPrice }
);

// Even for numbers and booleans
var sql = "SELECT * FROM Products WHERE IsActive = @IsActive AND Stock > @MinStock";
var products = await connection.QueryAsync<Product>(
    sql,
    new { IsActive = true, MinStock = 0 }
);
```

## Database User Permissions

### ❌ Don't Use root in Production
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod;User=root;Password=..."
  }
}
```

### ✅ Create Application-Specific User
```sql
-- Create user with limited permissions
CREATE USER 'productapi'@'%' IDENTIFIED BY 'strong_password';

-- Grant only necessary permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON ProductDB.* TO 'productapi'@'%';

-- Do NOT grant
-- GRANT ALL PRIVILEGES ON *.* TO 'productapi'@'%';
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod;Database=ProductDB;User=productapi;Password=..."
  }
}
```

## Connection String Options

### Development
```
Server=localhost;
Port=3306;
Database=ProductDB;
User=root;
Password=dev_password;
```

### Production
```
Server=prod-mysql.example.com;
Port=3306;
Database=ProductDB;
User=productapi;
Password=use_env_variable_or_keyvault;
SslMode=Required;
ConnectionTimeout=30;
Pooling=true;
MinimumPoolSize=5;
MaximumPoolSize=100;
```

## SSL/TLS Configuration

### ✅ Use SSL in Production
```
Server=prod-mysql.example.com;
Database=ProductDB;
User=productapi;
Password=...;
SslMode=Required;
```

### SSL Modes
- `None` - No SSL (❌ Not for production)
- `Preferred` - Use SSL if available
- `Required` - Require SSL (✅ Recommended for production)
- `VerifyCA` - Require SSL and verify CA
- `VerifyFull` - Require SSL and verify hostname

## Secrets Management Tools

### User Secrets (Development)
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
```

### Azure Key Vault (Production)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://myvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

### AWS Secrets Manager (Production)
```bash
aws secretsmanager create-secret \
    --name ProductDB/ConnectionString \
    --secret-string "Server=...;Password=..."
```

### HashiCorp Vault (Production)
```bash
vault kv put secret/productdb connection_string="Server=...;Password=..."
```

## Audit Checklist

- [ ] No passwords in appsettings.json
- [ ] appsettings.Development.json in .gitignore
- [ ] User secrets configured for local dev
- [ ] Environment variables used in production
- [ ] All queries use parameterized queries
- [ ] Database user has minimal required permissions
- [ ] SSL/TLS enabled for production
- [ ] Connection string not logged
- [ ] No connection string in error messages
- [ ] Secret rotation policy in place

## Common Mistakes to Avoid

1. ❌ Committing appsettings.Development.json with real passwords
2. ❌ Using root user in production
3. ❌ String concatenation in SQL queries
4. ❌ Logging connection strings
5. ❌ Hard-coding passwords in code
6. ❌ Not using SSL in production
7. ❌ Giving too many permissions to database user
8. ❌ Exposing connection strings in error messages

## Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Safe storage of app secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/)
- [OWASP SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection)
- [MySQL Security](https://dev.mysql.com/doc/refman/8.0/en/security.html)

---

**Security is not optional!** 🔒
