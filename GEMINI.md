# Project Overview

This project is a simplified version of a medical diagnostic software system, recently migrated from .NET Framework to .NET 8. It consists of a desktop monitoring application, a data ingestion API, and a device simulator. The primary goal is to modernize the architecture, implement MVVM and Dependency Injection in the desktop app, and extract domain logic into a reusable library.

## Architecture

- **IngestionApi (ASP.NET Core Minimal API):** Receives measurements from diagnostic devices and provides endpoints for querying data. Currently contains domain logic that needs to be extracted.
- **DesktopApp (WPF):** A monitor application that displays measurement data. Currently uses legacy patterns (code-behind, `WebClient`) and needs refactoring to MVVM and DI.
- **DeviceSimulator (Console App):** Simulates medical devices sending heart rate data to the IngestionApi.
- **Domain (TODO):** A planned class library to house shared domain models, validation logic, and interfaces.
- **IngestionApi.IntegrationTests (xUnit):** Integration tests for the API endpoints.

## Technologies

- .NET 8
- ASP.NET Core Minimal APIs
- WPF (Windows Presentation Foundation)
- HttpClient (for async communication)
- xUnit & Microsoft.AspNetCore.Mvc.Testing (for automated testing)
- Microsoft.Extensions.DependencyInjection (for DI)
- CommunityToolkit.Mvvm (recommended for MVVM)

# Building and Running

### Prerequisites
- .NET 8 SDK

### Build the Solution
```powershell
dotnet build
```

### Run the Ingestion API
```powershell
dotnet run --project src/IngestionApi/IngestionApi.csproj
```

### Run the Desktop App
```powershell
dotnet run --project src/DesktopApp/DesktopApp.csproj
```

### Run the Device Simulator
```powershell
dotnet run --project src/DeviceSimulator/DeviceSimulator.csproj
```

### Run Tests
```powershell
dotnet test
```

# Development Conventions

## Architectural Goals
1. **MVVM in DesktopApp:** Move UI logic from code-behind to ViewModels. Use bindings and commands.
2. **Dependency Injection:** Use `Microsoft.Extensions.DependencyInjection` in the `DesktopApp` to manage service lifetimes (e.g., `HttpClient`).
3. **Domain Extraction:** Extract `Measurement`, `IMeasurementStore`, and `MeasurementValidator` from `IngestionApi` to a new `Domain` project.
4. **Async/Await:** Prefer `HttpClient` over `WebClient`. Ensure all network and I/O operations are asynchronous.

## Testing Practices
- **Integration Tests:** Maintain and expand the `IngestionApi.IntegrationTests` project using `WebApplicationFactory`.
- **Unit Tests:** Add unit tests for domain logic (e.g., validators) and ViewModels.
- **Verification:** Always run `dotnet test` after changes to ensure no regressions.

## Coding Style
- Follow standard C# coding conventions.
- Use file-scoped namespaces.
- Prefer records for simple data structures (e.g., `Measurement`).
- Ensure proper error handling and logging (though logging is currently minimal).
