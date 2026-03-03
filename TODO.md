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

### [FEAT-02] User can delete a Rejected charge they created
**Page:** `MainPage` / `ExpenseDetailPage`
**Description:** A user should be able to delete a charge if and only if: (1) the charge status is `Rejected`, and (2) the current user is the charge creator (`charge.UserIdFk == currentUserId`). No other status should allow deletion.
**Fix area:**
- `ExpenseDetailPageModel`: add a `DeleteChargeCommand` guarded by the two conditions above. Call `ChargeRepository.DeleteAsync(id)` then navigate back.
- `MainPage` / `ExpenseItemView`: optionally surface a delete affordance (e.g. swipe or a button visible only when both conditions are met).
- Ensure RLS on the CHARGE table allows `DELETE` only for the row owner.

---

### [FEAT-03] A user can belong to multiple households; load the first one on login
**Page:** `MainPage` / `MainPageModel`, `HouseholdRepository`
**Description:** The data model and app logic must support a user appearing in more than one household (as `user_a_id_fk` or `user_b_id_fk`). On login, the first household returned from the DB is loaded by default. This is the foundation for the household switcher on `MainPage` — letting the user switch context and see their balance against a different partner.
**Fix area:**
- `HouseholdRepository`: change `GetByUserIdAsync` to `ListByUserIdAsync`, returning `List<HouseHoldDto>` ordered by `id` ascending (first = default).
- `MainPageModel`: store the full list; expose an `ActiveHousehold` and a `SwitchHouseholdCommand` for the switcher UI.
- `HouseholdSwitcher` control: wire up to allow switching when multiple households are available.

---

### [FEAT-04] Household must have a Name; display it in the HouseholdSwitcher
**Page:** `HouseholdSetupPage`, `MainPage` (`HouseholdSwitcher` control)
**Description:** The HOUSEHOLD table needs a `name` column. The user must provide this name when creating a household. It is what gets displayed in the `HouseholdSwitcher` on `MainPage` (currently showing the household `#Code`).
**Fix area:**
- **Supabase:** add `name TEXT NOT NULL` column to the `HOUSEHOLD` table. Update RLS as needed.
- `HouseHoldDto`: add `[Column("name")] public string Name { get; set; }` property.
- `HouseholdRepository`: include `name` in `INSERT` and `SELECT` statements.
- `HouseholdSetupPage` / `HouseholdSetupPageModel`: add a Name input field; validate non-empty before creating.
- `MainPageModel` / `HouseholdSwitcher`: display `household.Name` instead of `#Code`.

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
