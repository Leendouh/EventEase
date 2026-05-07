# EventEase Database Setup Guide

This guide helps you switch between local development (SSMS) and Azure production databases.

## Current Setup
- **Local Database**: EventEaseDB on SQL Server Express (SQLEXPRESS)
- **Azure Database**: eventdb111 on azuredb111.database.windows.net
- **Connection Files**: 
  - `appsettings.json` - Currently active configuration
  - `appsettings.azure.json` - Azure configuration backup

## Quick Switch Scripts

### Switch to Local Development
```powershell
.\switch-to-local.ps1
```
- Uses: `Server=localhost\\SQLEXPRESS;Database=EventEaseDB;Trusted_Connection=true;TrustServerCertificate=true;`
- Connects to your local SQL Server Express instance
- Perfect for development and testing with SSMS

### Switch to Azure Production
```powershell
.\switch-to-azure.ps1
```
- Uses: Azure SQL Database connection
- Connects to azuredb111.database.windows.net
- For production deployment and cloud testing

## Manual Connection Setup

### Local SQL Server Setup
1. Ensure SQL Server Express is running
2. Open SSMS and connect to `localhost\SQLEXPRESS` using Windows Authentication
3. Database `EventEaseDB` will be created automatically when you run the app

### Azure SQL Server Setup
1. Connect to `azuredb111.database.windows.net` in SSMS
2. Login: `azuredb111` with your password
3. Database: `eventdb111`

## Database Migration Commands

### Create New Migration
```bash
dotnet ef migrations add MigrationName
```

### Apply Migration to Current Database
```bash
dotnet ef database update
```

### Remove Last Migration
```bash
dotnet ef migrations remove
```

## Development Workflow

1. **Start Local Development**:
   ```powershell
   .\switch-to-local.ps1
   dotnet run
   ```

2. **Test with SSMS**:
   - Connect to `localhost\SQLEXPRESS`
   - Browse `EventEaseDB` database
   - View tables: Venues, Events, Bookings, etc.

3. **Switch to Azure for Production**:
   ```powershell
   .\switch-to-azure.ps1
   dotnet ef database update
   ```

## Important Notes

- All scripts automatically backup your current `appsettings.json` to `appsettings.json.backup`
- Local database uses Windows Authentication (no password needed)
- Azure database requires SQL Server authentication
- The application seeds initial data (venues, events, admin user) on first run

## Troubleshooting

### Local Database Issues
- Ensure SQL Server Express service is running
- Check that `SQLEXPRESS` instance is accessible
- Verify Windows Authentication permissions

### Azure Database Issues
- Check internet connection
- Verify Azure credentials are correct
- Ensure firewall allows access to Azure SQL

### Migration Issues
- Delete `Migrations` folder and recreate if needed
- Use `dotnet ef database drop` to reset completely
- Always backup data before dropping databases
