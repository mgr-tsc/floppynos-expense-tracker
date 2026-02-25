# TODO — Expense Tracker

Known issues and pending features, in rough priority order.

---

## Bugs

### [BUG-01] Reviewer can edit charge fields before approving/rejecting
**Page:** `ExpenseDetailPage`
**Description:** When User B opens a charge created by User A to approve or reject it, all form fields (Card, Amount, Split Policy, Category, Date) remain editable. Only the owner (User A) should be able to edit a charge.
**Expected:** Form fields are read-only when `IsExisting = true` and the current user is not the charge owner. Only the APPROVE / REJECT buttons should be actionable.
**Fix area:** `ExpenseDetailPage.xaml` — bind `IsEnabled` / `IsReadOnly` on each field to a new `IsOwner` computed property in `ExpenseDetailPageModel`.

---

### [BUG-02] Payment approval is failing
**Page:** `PaymentDetailPage`
**Description:** Approving a payment via the APPROVE button does not complete successfully. Exact failure mode (network error, RLS policy rejection, silent no-op) needs investigation.
**Fix area:** `PaymentDetailPageModel.Approve()` — add error logging, verify Supabase RLS policy allows `user_b` to update a payment row owned by `user_a`, and confirm `Status` field is being persisted correctly.

---

## Missing Features

### [FEAT-01] No way to create a new household once already in one
**Page:** `HouseholdSetupPage` is only reachable before a household exists.
**Description:** Once a user is assigned to a household there is no UI path to create a second one or switch households. The Settings page (`ManageProfilesPage`) is the natural home for this — currently a Coming Soon placeholder.
**Fix area:** Implement household management in `ManageProfilesPage` when Settings is built out. Consider adding a "Leave Household" option to trigger a re-route to `HouseholdSetupPage`.

---

### [FEAT-02] Rejected charges have no dedicated view
**Page:** `MainPage` — Charges section
**Description:** When User A's charge is rejected by User B the charge disappears from both users' views (or stays in "Pending" depending on filter). There is no "Rejected" tab in the status filter, so rejected charges are invisible.
**Expected UX:**
- Add a **Rejected** option to the `SfSegmentedControl` filter in the Charges section (`All | Pending | Approved | Rejected`).
- Rejected charges should display with a red left-accent stripe.
- The owner (User A) should see a clear "REJECTED" status badge and be able to edit and re-submit, or delete the charge.
**Fix area:** `MainPageModel.cs` (filter logic), `MainPage.xaml` (add Rejected segment), `ChargeDto.StatusColor` / `StatusLabel` computed properties, `ExpenseDetailPageModel` (re-submit flow).

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
