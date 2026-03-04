# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Floppyno's** — a .NET MAUI couples expense tracker app (`com.keepersoft.ExpenseTracker`).
Two users share a household and split expenses according to configurable policies (50/50, 70/30, etc.). Charges and payments go through an approval workflow (pending → approved/rejected). Balance is calculated server-side via Supabase RPC.

**Data flow:** `View (XAML) → PageModel → Repository → Supabase (PostgreSQL + RLS)`

## Build & Run Commands

```bash
# Versioned TFMs are required — bare names like net10.0-ios will fail
dotnet build ExpenseTracker/ExpenseTracker.csproj -f net10.0-maccatalyst26.2
dotnet build ExpenseTracker/ExpenseTracker.csproj -f net10.0-ios26.2
dotnet build ExpenseTracker/ExpenseTracker.csproj -f net10.0-android

dotnet run --project ExpenseTracker/ExpenseTracker.csproj -f net10.0-maccatalyst26.2
dotnet restore ExpenseTracker/ExpenseTracker.csproj
```

## Architecture

**Pattern:** MVVM + Repository + DI. No EF Core; all data goes through Supabase SDK.

### Layers

- **Models/Supabase/** — Supabase DTOs decorated with `[MapTo]`/`[PrimaryKey]` attributes and Newtonsoft.Json serialization. Computed properties (e.g., `DisplayAmount`, `StatusColor`) are marked `[Newtonsoft.Json.JsonIgnore]`. Never use `System.Text.Json.Serialization` attributes on these.
- **Models/** — Local-only records: `BalanceData`, `HouseHoldCreationRecord`, `Settings`.
- **Data/Repositories/** — One repository per domain entity. Each wraps Supabase SDK calls (Postgrest queries or RPC). All are registered as singletons.
- **Data/AppSettings.cs** — Loads `appsettings.json` + `supabase.env` from `Resources/Raw/` at startup. Exposes static properties: `SupabaseUrl`, `SupabaseKey`, `SupabaseAnonKey`, `GoogleWebClientId`.
- **Services/SupabaseService.cs** — Lazily initializes the Supabase client. Key methods: `InitializeAsync()`, `GetCurrentUserId()`, `CreateHouseHoldTrackerAsync(name)` (RPC), `GetHouseholdBalanceAsync(householdId)` (RPC).
- **Services/GoogleSignIn.cs** — Implements `ISigInInThirdParty`. Google OAuth via `WebAuthenticator`. Session is restored from `SecureStorage` (Release) or `Preferences` (Debug) via `TokenStorage.cs`.
- **Services/ModalErrorHandler.cs** — Implements `IErrorHandler`. Async-safe error display using `SemaphoreSlim`.
- **PageModels/** — ViewModels using `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`).
- **Pages/** — XAML views. `Pages/Controls/` has reusable components (CustomTabBar, FilterPanel, ExpenseItemView, PaymentItemView, AddButton, BalanceView, HeaderBar, HouseholdSwitcher).
- **Utilities/TaskUtilities.cs** — `FireAndForgetSafeAsync(IErrorHandler)` for safe async void patterns; `IsNullOrNew()` extension.

### Navigation

Shell-based. `Shell.NavBarIsVisible="False"` on full-screen pages; `CustomTabBar` is overlaid in a single-row Grid.

- Main tabs (LoadingPage, SignInPage, HouseholdSetupPage, MainPage, CardDetailPage, ManageProfilesPage) — registered as **singletons**.
- Detail routes registered as **transients** with Shell routes:
  - `AddTransientWithShellRoute<ExpenseDetailPage, ExpenseDetailPageModel>("expense")`
  - `AddTransientWithShellRoute<PaymentDetailPage, PaymentDetailPageModel>("payment")`

Data passes through query parameters and `IQueryAttributable`.

### DI Registration (`MauiProgram.cs`)

```
Singletons: ModalErrorHandler, ISigInInThirdParty (→GoogleSignIn), SupabaseService
            CardRepository, ChargeRepository, PolicyRepository,
            ChargeCategoryRepository, HouseholdRepository, PaymentRepository
            LoadingPage/Model, SignInPage/Model, MainPage/Model,
            CardDetailPage/Model, CardListPage/Model,
            ManageProfilesPage/Model, HouseholdSetupPage/Model, MockHomePage

Transients: ExpenseDetailPage/Model ("expense"), PaymentDetailPage/Model ("payment")
```

### Household Model

Each household has exactly two members (`user_a_id_fk`, `user_b_id_fk`). `HouseholdRepository.GetByUserIdAsync(userId)` checks both columns. Joining uses a numeric code stored in `HouseHoldDto`. Balance is calculated via Supabase RPC `calculate_household_balance`.

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.3.2 | MVVM source generators |
| CommunityToolkit.Maui | 12.3.0 | UI behaviors, converters, Toast, Snackbar |
| Syncfusion.Maui.Toolkit | 1.0.7 | SfTextInputLayout, SfPullToRefresh, Charts |
| Supabase | 1.1.1 | Supabase client (Postgrest + Auth + RPC) |
| Microsoft.Data.Sqlite.Core | 8.0.8 | SQLite (retained; not primary data layer) |
| SQLitePCLRaw.bundle_green | 2.1.10 | SQLite native binding |

## iOS Device Signing

Signing config is split to keep personal identifiers out of git:

- **`.csproj`** (committed) — `ProvisioningType=Automatic` and `CodesignEntitlements` path only.
- **`.csproj.user`** (gitignored) — `CodesignKey` and `TeamIdentifier`. Every developer must create this file locally.

```xml
<!-- ExpenseTracker.csproj.user — never commit -->
<Project>
  <PropertyGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
    <CodesignKey>Apple Development: your@email.com (TEAMID)</CodesignKey>
    <TeamIdentifier>TEAMID</TeamIdentifier>
  </PropertyGroup>
</Project>
```

Always use forward slashes in `CodesignEntitlements` paths — backslashes fail on macOS.
See `docs/iOS-Device-Deployment-Guide.md` for the full diagnosis and prevention checklist.

## Conventions

### Global Usings (`GlobalUsings.cs`)
```
ExpenseTracker.Data
ExpenseTracker.Data.Repositories
ExpenseTracker.Models
ExpenseTracker.PageModels
ExpenseTracker.Pages
ExpenseTracker.Services
ExpenseTracker.Utilities
```
The `Models.Supabase` sub-namespace is **not** globally imported — use fully-qualified or add a using where needed.

### Supabase / JSON
- All DTOs are in `Models/Supabase/`. Use `[MapTo("column")]` for column mapping.
- Computed properties must be annotated `[Newtonsoft.Json.JsonIgnore]` (Supabase uses Newtonsoft, not System.Text.Json).
- Linter renames DTOs to PascalCase acronyms: `CardDTO → CardDto`, `PaymentDTO → PaymentDto`.

### XAML / Bindings
- All pages set `x:DataType` for compiled bindings.
- `Picker` controls: bind `SelectedIndex`, NOT `SelectedItem` (avoids dropdown-close bug).
- `Picker` inside `SfTextInputLayout` causes closing bugs — use a plain `Label + Picker` instead.
- `Border` has no `CornerRadius` attribute — always use:
  ```xml
  <Border.StrokeShape><RoundRectangle CornerRadius="16" /></Border.StrokeShape>
  ```
- `EventToCommandBehavior` requires `Path=BindingContext`:
  ```xml
  BindingContext="{Binding Path=BindingContext, Source={x:Reference PageName}, x:DataType=ContentPage}"
  ```
- Custom controls (e.g., AddButton): bind commands directly `{Binding CommandName}` — avoid `Source={x:Reference}` on custom control internals.

### Animation
Use `TranslateToAsync` / `FadeToAsync` (NOT `TranslateTo` / `FadeTo` — obsolete in .NET 10).

### Async
All PageModel async operations use `FireAndForgetSafeAsync(_errorHandler)`. Never use bare `async void`.

### App Startup Flow
`LoadingPage` → calls `AppSettings.InitializeAsync()` → restores Supabase session → routes to:
- `SignInPage` (no session)
- `HouseholdSetupPage` (session but no household)
- `MainPage` (session + household)

## Design System

**Theme:** Forced dark on all platforms (set in `AppShell` constructor: `UserAppTheme = AppTheme.Dark`).

### Colors (`Resources/Styles/Colors.xaml`)

| Key | Value | Use |
|-----|-------|-----|
| AppBackground | `#111111` | Page backgrounds |
| CardSurface | `#1C1C1C` | Card/panel backgrounds |
| ElevatedSurface | `#282828` | Raised elements |
| StrokeColor | `#2A2A2A` | Borders, dividers |
| GoldAccent | `#D4AF37` | Primary accent, active tab |
| MutedText | `#666666` | Inactive labels |
| DimmedText | `#A0A0A0` | Secondary text |
| LightOnDarkBackground | `#C3C3C3` | Body text on dark |

### Named Styles (`Resources/Styles/AppStyles.xaml`)

| Key | Description |
|-----|-------------|
| `DarkCard` | `Border` with CardSurface bg, StrokeColor stroke, 16px radius |
| `SectionLabelStyle` | SpaceGrotesk 10pt, uppercase, MutedText, 2px char spacing |
| `HeroAmountStyle` | JetBrainsMono-Bold 36pt, white |
| `TabLabelStyle` | SpaceGrotesk-SemiBold 9pt, uppercase, 0.5px char spacing |
| `CardStyle` | Border variant with rounded corners |

### Typography
- **Sans:** SpaceGrotesk (Regular / SpaceGrotesk-SemiBold / SpaceGrotesk-Bold)
- **Mono:** JetBrainsMono (JetBrainsMono / JetBrainsMono-Bold)

### Layout Patterns
- **Left-accent stripe:** `Border Style="DarkCard" Padding="0"` → `Grid ColumnDefinitions="3,*"` → `BoxView` (GoldAccent) in col 0.
- **Full-screen pages:** `Shell.NavBarIsVisible="False"` on the `ContentPage`.
- **CustomTabBar:** floating `ContentView` with `ActiveTabIndex` `BindableProperty`. Tabs: Dashboard (`//main`), Cards (`//cards`), Settings (`//settings`). Gold background + dark text when active; transparent + muted text when inactive.

### Icons
FluentUI system icons via `FluentUI` static class and `FluentSystemIcons-Regular.ttf` font.
Named icon resources in AppStyles.xaml: `IconDashboard`, `IconCards`, `IconSettings`, `IconMoney`, `IconAdd`, `IconDelete`, etc.

### Global Notifications
`AppShell.DisplaySnackbarAsync(msg)` — error/alert snackbar (red bg).
`AppShell.DisplayToastAsync(msg)` — informational toast (skipped on Windows).

## Supabase Backend Notes

- RLS policies use `auth.uid()` for user isolation — no `SECURITY DEFINER` bypasses.
- `supabase.env` (in `Resources/Raw/`) holds credentials. Never commit real credentials; use `supabase.env.example` as template.
- Household creation uses the RPC `create_household_with_temp_record` (returns uuid + join code).
- Balance uses the RPC `calculate_household_balance` (returns `BalanceData`).
- Charges and Payments follow an approval workflow: `pending → approved / rejected`.
