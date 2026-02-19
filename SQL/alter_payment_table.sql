-- Run this in the Supabase SQL Editor
-- Adds household, status, and payment_method columns to PAYMENT table

ALTER TABLE "PAYMENT" ADD COLUMN household_id_fk BIGINT REFERENCES "HOUSEHOLD"(id);
ALTER TABLE "PAYMENT" ADD COLUMN status TEXT NOT NULL DEFAULT 'pending';
ALTER TABLE "PAYMENT" ADD COLUMN payment_method TEXT NOT NULL DEFAULT 'cash';
-- payment_method values: 'transfer_paypal', 'transfer_zelle', 'transfer_applepay', 'check', 'cash'
