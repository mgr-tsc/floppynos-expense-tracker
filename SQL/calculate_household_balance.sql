-- Run this in the Supabase SQL Editor
-- Calculates how much each household member owes the other

CREATE OR REPLACE FUNCTION calculate_household_balance(p_household_id BIGINT)
RETURNS JSON
LANGUAGE plpgsql
SECURITY INVOKER
AS $$
DECLARE
    v_user_a_id UUID;
    v_user_b_id UUID;
    v_user_a_name TEXT;
    v_user_b_name TEXT;
    v_user_a_owes NUMERIC := 0;
    v_user_b_owes NUMERIC := 0;
    v_net NUMERIC;
    v_summary TEXT;
    rec RECORD;
BEGIN
    -- Get household members
    SELECT user_a_id_fk, user_b_id_fk
    INTO v_user_a_id, v_user_b_id
    FROM "HOUSEHOLD"
    WHERE id = p_household_id;

    IF v_user_a_id IS NULL THEN
        RETURN json_build_object(
            'user_a_name', '',
            'user_a_owes', 0,
            'user_b_name', '',
            'user_b_owes', 0,
            'summary', 'Household not found'
        );
    END IF;

    -- Get user names from auth.users
    SELECT COALESCE(raw_user_meta_data->>'name', email) INTO v_user_a_name
    FROM auth.users WHERE id = v_user_a_id;

    IF v_user_b_id IS NOT NULL THEN
        SELECT COALESCE(raw_user_meta_data->>'name', email) INTO v_user_b_name
        FROM auth.users WHERE id = v_user_b_id;
    ELSE
        v_user_b_name := 'Partner';
    END IF;

    -- Calculate what each person owes based on approved charges
    FOR rec IN
        SELECT
            c.id AS charge_id,
            c.amount,
            c.user_id_fk AS paid_by,
            p.user_a_percentage,
            p.user_b_percentage,
            COALESCE(pay.total_paid, 0) AS total_paid
        FROM "CHARGE" c
        JOIN "POLICY" p ON c.policy_id_fk = p.id
        LEFT JOIN (
            SELECT charge_id_fk, SUM(amount) AS total_paid
            FROM "PAYMENT"
            GROUP BY charge_id_fk
        ) pay ON pay.charge_id_fk = c.id
        WHERE c.household_id_fk = p_household_id
          AND c.status = 'approved'
    LOOP
        IF rec.paid_by = v_user_a_id THEN
            -- user_a paid, so user_b owes their share minus payments
            v_user_b_owes := v_user_b_owes + (rec.amount * rec.user_b_percentage / 100) - rec.total_paid;
        ELSIF rec.paid_by = v_user_b_id THEN
            -- user_b paid, so user_a owes their share minus payments
            v_user_a_owes := v_user_a_owes + (rec.amount * rec.user_a_percentage / 100) - rec.total_paid;
        END IF;
    END LOOP;

    -- Ensure no negative values
    v_user_a_owes := GREATEST(v_user_a_owes, 0);
    v_user_b_owes := GREATEST(v_user_b_owes, 0);

    -- Build summary
    v_net := v_user_a_owes - v_user_b_owes;
    IF v_net > 0 THEN
        v_summary := v_user_a_name || ' owes $' || TRIM(TO_CHAR(v_net, '999999990.00'));
    ELSIF v_net < 0 THEN
        v_summary := v_user_b_name || ' owes $' || TRIM(TO_CHAR(ABS(v_net), '999999990.00'));
    ELSE
        v_summary := 'All settled up!';
    END IF;

    RETURN json_build_object(
        'user_a_name', v_user_a_name,
        'user_a_owes', v_user_a_owes,
        'user_b_name', v_user_b_name,
        'user_b_owes', v_user_b_owes,
        'summary', v_summary
    );
END;
$$;
