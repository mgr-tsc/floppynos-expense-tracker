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

### [FEAT-05] Invite partner via Copy Code CTA
**Page:** Settings / `ManageProfilesPage` *(tentative)*
**Description:** Allow a household owner to share their household code so a second user can join. The feature has two parts:

**Part 1 — Supabase helper function**
Create a boolean RPC `is_household_full` that returns `true` when both `user_a_id_fk` and `user_b_id_fk` are non-null, and `false` otherwise. Accept either the household `id` (bigint) or `name` (text) as the argument (two overloads or a single function with optional params — TBD):
```sql
-- Example signature (by id)
CREATE OR REPLACE FUNCTION public.is_household_full(p_household_id bigint)
RETURNS boolean ...
```

**Part 2 — Copy Code CTA**
Add a "Copy Code" button to the Settings page (or wherever household info is surfaced). The button:
- Is **enabled** only when `is_household_full` returns `false` (household has an open slot)
- Copies `household.Code` to the clipboard when tapped
- Shows a brief confirmation (e.g. toast "Code copied!")

**Fix area:**
- **Supabase:** implement `is_household_full` RPC with RLS-safe access (caller must belong to the household).
- `SupabaseService`: add `IsHouseholdFullAsync(long householdId)` wrapping the RPC call.
- `ManageProfilesPage` / its PageModel *(page TBD)*: call `IsHouseholdFullAsync`, expose `CanCopyCode` bool, wire up `CopyCodeCommand` using `Clipboard.SetTextAsync`.

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
