# Database Configuration Guide

## Overview

This application uses **SQL Server** as its database. The configuration differs between local development and cloud deployment.

## Local Development (Windows)

For local development, the application uses **SQL Server LocalDB**, which is automatically installed with Visual Studio.

Connection strings are configured in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "RelyfDb": "Server=(localdb)\\ProjectModels;Database=Relyf.Database;Trusted_Connection=True;MultipleActiveResultSets=true",
    "Default": "Server=(localdb)\\ProjectModels;Database=Relyf.Database;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

### Verifying LocalDB

To check if LocalDB is running:

```powershell
sqllocaldb info
sqllocaldb start ProjectModels
```

## Cloud Deployment (Render, Azure, etc.)

**LocalDB does NOT work on Linux servers like Render.** You must use a cloud-hosted SQL Server database.

### Option 1: Azure SQL Database (Recommended)

1. **Create Azure SQL Database:**
   - Go to [Azure Portal](https://portal.azure.com)
   - Create a new SQL Database
   - Configure firewall to allow connections from Render IPs
   - Note: Azure SQL Database has a free tier for small projects

2. **Get Connection String:**
   ```
   Server=tcp:your-server.database.windows.net,1433;
   Initial Catalog=RelyfDb;
   Persist Security Info=False;
   User ID=your_username;
   Password=your_password;
   MultipleActiveResultSets=False;
   Encrypt=True;
   TrustServerCertificate=False;
   Connection Timeout=30;
   ```

3. **Set Environment Variables on Render:**
   - Go to your Render service → Environment
   - Add these variables:
     ```
     ConnectionStrings__Default=<your-azure-sql-connection-string>
     ConnectionStrings__RelyfDb=<your-azure-sql-connection-string>
     ```

### Option 2: Other SQL Server Hosting Providers

Alternative hosting providers for SQL Server:

- **Somee.com** - Free SQL Server hosting
- **SmarterASP.NET** - Affordable SQL Server hosting
- **GearHost** - Cloud SQL Server hosting

Use the same environment variable configuration as Azure SQL.

### Option 3: PostgreSQL (Requires Code Changes)

If you want to use PostgreSQL instead of SQL Server:

1. **Install NuGet packages:**
   ```bash
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

2. **Update Program.cs:**
   ```csharp
   builder.Services.AddDbContext<RelyfDbContext>(opt =>
       opt.UseNpgsql(builder.Configuration.GetConnectionString("RelyfDb")));
   ```

3. **Use Render's PostgreSQL:**
   - Create a PostgreSQL database in Render
   - Render provides the connection string automatically

## Environment Variable Configuration

### Required Environment Variables for Production

```bash
# Database
ConnectionStrings__Default=<your-sql-server-connection-string>
ConnectionStrings__RelyfDb=<your-sql-server-connection-string>

# JWT Authentication
Jwt__Key=<base64-encoded-64-byte-secret>
Jwt__Issuer=Relyf
Jwt__Audience=Relyf.Client

# Cohere AI
Cohere__ApiKey=<your-cohere-api-key>

# CORS (already configured in appsettings.json)
```

### Setting Environment Variables on Render

1. Go to your service in Render Dashboard
2. Navigate to **Environment** tab
3. Click **Add Environment Variable**
4. Add each variable with its value
5. Click **Save Changes**
6. Render will automatically redeploy

## Connection String Format

### SQL Server (Azure, Cloud Providers)
```
Server=tcp:server-name.database.windows.net,1433;Initial Catalog=DatabaseName;User ID=username;Password=password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### SQL Server (Local Development)
```
Server=(localdb)\\ProjectModels;Database=Relyf.Database;Integrated Security=true;TrustServerCertificate=true
```

### PostgreSQL (if migrating)
```
Host=hostname;Database=dbname;Username=user;Password=password;SSL Mode=Require;
```

## Troubleshooting

### Error: "Cannot connect to LocalDB on Render"
- **Cause:** LocalDB only works on Windows
- **Solution:** Set up cloud SQL Server database and configure environment variables

### Error: "Login failed for user"
- **Cause:** Incorrect credentials or firewall blocking connection
- **Solution:** 
  - Verify connection string credentials
  - Configure database firewall to allow Render's IP addresses
  - For Azure SQL: Add "Allow Azure services" firewall rule

### Error: "Connection timeout"
- **Cause:** Database server not accessible or firewall blocking
- **Solution:**
  - Verify database server is running
  - Check firewall rules
  - Increase Connection Timeout in connection string

## Database Migrations

To apply migrations to your cloud database:

### From Local Development
```bash
# Update connection string in appsettings.json temporarily
dotnet ef database update --project Relyf.Api

# Or use environment variable
$env:ConnectionStrings__Default="<cloud-connection-string>"
dotnet ef database update --project Relyf.Api
```

### Using SQL Scripts
1. Generate SQL script from migrations:
   ```bash
   dotnet ef migrations script --project Relyf.Api --output migration.sql
   ```

2. Run the script against your cloud database using:
   - Azure Data Studio
   - SQL Server Management Studio (SSMS)
   - Azure Portal Query Editor

## Security Best Practices

1. **Never commit connection strings** with real passwords to Git
2. **Use environment variables** for all sensitive configuration
3. **Use Azure Key Vault** or similar for production secrets
4. **Rotate passwords** regularly
5. **Use least-privilege** database users (not `sa` or admin accounts)
6. **Enable SSL/TLS encryption** for database connections
7. **Restrict firewall rules** to only necessary IP ranges

## Next Steps

1. ✅ Choose a cloud SQL Server provider (Azure SQL recommended)
2. ✅ Create database and obtain connection string
3. ✅ Add environment variables to Render
4. ✅ Apply database migrations
5. ✅ Test the deployment

For questions or issues, check the Render logs:
```
Render Dashboard → Your Service → Logs
```
