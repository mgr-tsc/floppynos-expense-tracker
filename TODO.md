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


## Notes
- RLS policies must be verified for all new DB operations (charges, payments, households). No `SECURITY DEFINER` bypasses.
- All async operations in PageModels must use `FireAndForgetSafeAsync(_errorHandler)`.
