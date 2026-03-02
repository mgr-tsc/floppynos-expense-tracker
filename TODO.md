# TODO — Expense Tracker

Known issues and pending features, in rough priority order.

---

## Bugs

### [BUG-04b] * No DB-level constraint preventing future dates on CHARGE and PAYMENT
**Description:** Even with app-level validation in place, there is no database constraint stopping a future `date` value from being inserted directly (e.g. via API or SQL editor).
**Fix area:** Add a `CHECK` constraint to the CHARGE and PAYMENT tables in Supabase: `CHECK (date <= CURRENT_DATE)`.
**Note:** Low priority — app-level validation is sufficient for normal use.

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

### [DESIGN-03] Sync Pencil design file with current MainPage implementation
**Description:** The Pencil design tool is out of date and does not reflect the MainPage as it currently exists (dark theme, CustomTabBar, filter tabs, BalanceView, collapsible Charges/Payments sections). Update the design file to match the implemented UI so it can be used as the source of truth for future design decisions.
**Deliverable:** Updated Pencil screens for MainPage matching the live implementation.

---

### [DESIGN-04] Date filtering for Charges and Payments on MainPage
**Page:** `MainPage` / `MainPageModel`
**Description:** Add the ability to filter the Charges and Payments lists by date range. By default, only records from the **last 2 months** should be loaded. Users should be able to adjust the date range via a filter control (e.g. a date range picker or preset chips: Last Month / Last 3 Months / All Time).
**Fix area:**
- `MainPageModel`: apply a default date filter (`DateFrom = DateTime.Now.AddMonths(-2)`) when calling `ListByHouseholdAsync`. Add observable `DateFrom` / `DateTo` properties and re-filter on change.
- `ChargeRepository` / `PaymentRepository`: add an overload or parameter to `ListByHouseholdAsync` that accepts a date range.
- `MainPage.xaml`: add date filter UI above or alongside the status filter tabs.

---

## Notes
- RLS policies must be verified for all new DB operations (charges, payments, households). No `SECURITY DEFINER` bypasses.
- All async operations in PageModels must use `FireAndForgetSafeAsync(_errorHandler)`.
