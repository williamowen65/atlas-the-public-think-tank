# Shared.Identity

This project contains the shared identity components for Atlas: The Public Think Tank. It centralizes authentication, authorization, and user management features.

## Overview

The `Shared.Identity` project provides a centralized identity system that can be used across multiple applications in the mono repo, including:

- Identity pages (login, registration, account management)
- Identity UI extensions
- Authentication and authorization services

## Usage

To use the shared identity components in a client application:

1. Add a reference to the Shared.Identity project:
```xml
<ProjectReference Include="..\Shared.Identity\Shared.Identity.csproj" />
```

2. Configure Identity in your Program.cs:
```csharp
// Add shared identity database (also requires reference to Shared.DbContext)
builder.Services.AddSharedIdentityDatabase(builder.Configuration);

// Add shared identity UI
builder.Services.AddSharedIdentityUI(builder.Configuration, options => {
    options.SignIn.RequireConfirmedAccount = true;
    // Add any additional configuration here
});

// In middleware section
app.UseSharedIdentityUI();
```

## Identity Pages

The Shared.Identity project contains the following identity pages:

- Login
- Register
- Manage Account
- Password Reset
- Email Confirmation
- Two-Factor Authentication

## Customizing Identity

You can customize the identity behavior by passing options to the `AddSharedIdentityUI` method:

```csharp
builder.Services.AddSharedIdentityUI(builder.Configuration, options => {
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    // User settings
    options.User.RequireUniqueEmail = true;
});
```