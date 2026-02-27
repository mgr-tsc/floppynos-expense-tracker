# TODO — Expense Tracker

Known issues and pending features, in rough priority order.

---

## Bugs

### [UI-01] Back button arrow renders incorrectly on iOS
**Affected pages:** All detail pages (`ExpenseDetailPage`, `PaymentDetailPage`, `CardDetailPage`, etc.)
**Description:** The back button (circular Border containing a FluentUI `arrow_left_24_regular` glyph) does not render correctly on physical iOS devices. Likely a font glyph alignment or Border clipping issue specific to iOS.
**Fix area:** Investigate `RoundRectangle CornerRadius` clipping + FluentUI icon vertical alignment on iOS. May need `Padding`, `LineHeight`, or a platform-specific adjustment across all pages using the back button pattern.

---

### [UI-02] User avatar renders incorrectly on iOS
**Affected pages:** `MainPage` (via `HeaderBar` control)
**Description:** The gold circle avatar (44×44 `Border` with `CornerRadius 999` containing the user's initials label) does not render correctly on physical iOS devices. Likely the same Border/clipping issue as UI-01.
**Fix area:** `Pages/Controls/HeaderBar.xaml` — investigate `RoundRectangle` clipping on iOS. May need to switch to an `Ellipse` shape or apply platform-specific sizing.

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
