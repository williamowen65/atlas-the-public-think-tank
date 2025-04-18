# Shared.UI

This project contains shared UI components for Atlas: The Public Think Tank. It centralizes common UI elements, JavaScript functionality, and styling.

## Overview

The `Shared.UI` project provides shared UI assets and components that can be used across multiple applications in the mono repo, including:

- JavaScript files
- CSS styles
- Reusable Razor components and views
- UI helpers and extensions

## Usage

To use the shared UI components in a client application:

1. Add a reference to the Shared.UI project:
```xml
<ProjectReference Include="..\Shared.UI\Shared.UI.csproj" />
```

2. Configure shared UI in your Program.cs:
```csharp
// Add shared UI services
builder.Services.AddSharedUI();

// In middleware section
app.UseSharedUI();
```

3. Reference shared JavaScript files in your views:
```html
<script src="/shared-ui/js/site.js"></script>
<script src="/shared-ui/js/color-modes.js"></script>
<script src="/shared-ui/js/ajax_crud.js"></script>
```

## JavaScript Components

The Shared.UI project contains the following JavaScript files:

- **site.js** - Base JavaScript functionality
- **color-modes.js** - Theme toggling between light and dark modes
- **ajax_crud.js** - CRUD operations using AJAX for merchandise management

## Future Extensions

Consider adding the following to the Shared.UI project:

- Shared CSS styles
- Shared Razor components
- Shared layout templates
- Custom tag helpers
- Shared view components