# Shared.DbContext

This project contains the shared database context and Entity Framework Core migrations for Atlas: The Public Think Tank.

## Overview

The `Shared.DbContext` project provides a centralized database access layer using Entity Framework Core that can be used across multiple applications in the mono repo. This includes:

- ApplicationDbContext for database access
- Entity Framework Core migrations
- Data models and configurations

## Usage

To use this shared data layer in a client application:

1. Add a reference to the Shared.DbContext project:
```xml
<ProjectReference Include="..\Shared.DbContext\Shared.DbContext.csproj" />
```

2. Configure the database in your Program.cs or Startup.cs:
```csharp
// Add database using the shared context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

3. To create a new migration:
```
dotnet ef migrations add MyMigration --project Shared.DbContext --startup-project WebClient
```

4. To update the database:
```
dotnet ef database update --project Shared.DbContext --startup-project WebClient
```

## Entity Framework Core Migrations

Migrations are version-controlled database schema changes. For consistency, manage migrations using WebClient as the startup project, but store them in this Shared.DbContext project.