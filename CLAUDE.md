# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A .NET MAUI couple expense tracker app. The current codebase is scaffolded from a project management template and will be adapted for expense tracking. Maintain the same organizational patterns when building new features.

## Build & Run Commands

```bash
# Build for macCatalyst (default on macOS)
dotnet build ExpenseTracker/ExpenseTracker.csproj -f net10.0-maccatalyst

# Build for iOS / Android
dotnet build ExpenseTracker/ExpenseTracker.csproj -f net10.0-ios
dotnet build ExpenseTracker/ExpenseTracker.csproj -f net10.0-android

# Run on macCatalyst
dotnet run --project ExpenseTracker/ExpenseTracker.csproj -f net10.0-maccatalyst

# Restore NuGet packages
dotnet restore ExpenseTracker/ExpenseTracker.csproj
```

Target framework is `net10.0`. No test project exists yet.

## Architecture

**Pattern:** MVVM with Repository pattern and DI, following `View → PageModel → Repository → SQLite` data flow.

**Key layers:**
- **Models/** — POCOs with computed properties (e.g., `ColorBrush` on `Category`/`Tag`). No ORM attributes; schema is defined in repository SQL.
- **Data/** — Repository classes with raw SQLite via `Microsoft.Data.Sqlite`. Each repository has an `Init()` method that creates tables on first use. `SeedDataService` loads `Resources/Raw/SeedData.json` on first launch (tracked via `Preferences`).
- **PageModels/** — ViewModels using `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`). These are the binding context for pages.
- **Pages/** — XAML views with code-behind. `Pages/Controls/` has reusable components.
- **Services/** — `ModalErrorHandler` implements `IErrorHandler` for async-safe error display with `SemaphoreSlim`.
- **Utilities/** — `FireAndForgetSafeAsync()` for safe async void patterns; `IsNullOrNew()` extension for entity existence checks.

**Navigation:** Shell-based with flyout. Main tabs registered as singletons. Detail pages registered as transients via `AddTransientWithShellRoute<TPage, TPageModel>("route")`. Data passes through query parameters and `IQueryAttributable`.

**DI registration** is in `MauiProgram.cs`:
- Singletons: repositories, main page models, services
- Transients: detail pages (`ProjectDetailPage`, `TaskDetailPage`) — new instance per navigation

**Database:** SQLite at `FileSystem.AppDataDirectory/AppSQLite.db3`. Connection string in `Data/Constants.cs`. Repositories use parameterized queries and `await using` disposal.

## Key Dependencies

| Package | Purpose |
|---------|---------|
| CommunityToolkit.Mvvm (8.3.2) | MVVM source generators |
| CommunityToolkit.Maui (12.3.0) | UI behaviors, converters |
| Syncfusion.Maui.Toolkit (1.0.7) | Charts, PullToRefresh, SegmentedControl, TextInputLayout |
| Microsoft.Data.Sqlite.Core (8.0.8) | SQLite access (no EF Core) |

## Conventions

- Global usings are declared in `GlobalUsings.cs` — all namespaces (`Data`, `PageModels`, `Pages`, `Services`, `Utilities`, `Fonts`) are globally imported.
- Icons use FluentUI system icons via the `FluentUI` static class and the `FluentSystemIcons-Regular.ttf` font.
- Color theming supports light/dark mode, toggled in `AppShell.xaml.cs` flyout footer.
- XAML pages set `x:DataType` for compiled bindings.
- Async methods in PageModels use `FireAndForgetSafeAsync(IErrorHandler)` to avoid unobserved exceptions.