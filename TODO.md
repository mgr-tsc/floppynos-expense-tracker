# TODO — Expense Tracker

Known issues and pending features, in rough priority order.

---

## Bugs


### [BUG-03] Balance calculations are incorrect
**Affected pages:** `MainPage` (via `BalanceView`)
**Description:** The "who owes who" figures shown in the balance card do not reflect actual charge/payment data. Totals appear wrong or static. Root cause likely spans multiple layers:
- `BalanceData` computation logic in `MainPageModel` may not aggregate correctly across all charges and payments.
- Supabase RLS policies on CHARGE and PAYMENT tables may prevent the current user from reading the partner's records, causing one-sided totals.
- Settlement logic (approved payments reducing the owed amount) may not be applied.
**Fix area:** Audit `LoadBalanceAsync()` in `MainPageModel.cs`. Verify RLS allows both household members to read each other's records. Add integration checks for edge cases (no charges, unequal splits, partial payments).

---


## Missing Features

### [FEAT-01] No way to create a new household once already in one
**Page:** `HouseholdSetupPage` is only reachable before a household exists.
**Description:** Once a user is assigned to a household there is no UI path to create a second one or switch households. The Settings page (`ManageProfilesPage`) is the natural home for this — currently a Coming Soon placeholder.
**Fix area:** Implement household management in `ManageProfilesPage` when Settings is built out. Consider adding a "Leave Household" option to trigger a re-route to `HouseholdSetupPage`.

---

## Pending Pages / Modules

### [PAGE-01] Settings page not yet implemented
**Page:** `ManageProfilesPage` — currently shows a Coming Soon placeholder.
**Planned content (from Pencil `9Z0GG` design):**
- Profile avatar upload / initials display
- Default household selector
- Save / Reset app actions
**Blocked by:** No `UserProfileRepository` or profile DB table yet.

---

## Design Tasks

### [DESIGN-02] Design app icon
**Assets:** `Resources/AppIcon/appicon.svg` (background) + `Resources/AppIcon/appiconfg.svg` (foreground)
**Description:** The current app icon is the default .NET MAUI template icon. A custom icon is needed that reflects the app identity — ideally using the gold `#D4AF37` accent on a dark `#111111` background.
**Deliverable:** Replace `appicon.svg` and/or `appiconfg.svg` with a custom design. The background color in the csproj is currently `#17171a` (update to `#111111` when replacing). Rebuild required.

---

## Notes
- RLS policies must be verified for all new DB operations (charges, payments, households). No `SECURITY DEFINER` bypasses.
- All async operations in PageModels must use `FireAndForgetSafeAsync(_errorHandler)`.
