/* Drop Tables */

DROP TABLE IF EXISTS public.transaction CASCADE
;

/* Create Tables */

CREATE TABLE public.transaction
(
	transaction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
	user_id UUID NOT NULL,	-- FK to user. Owner of the transaction.
	loyalty_program_id integer NOT NULL,	-- FK to loyalty_program. Points program tied to this transaction.
	transaction_description varchar(256) NOT NULL,	-- Description of the purchased item. Ex: TV.
	transaction_store varchar(256) NOT NULL,	-- Name of the store or retailer. Ex: Casas Bahia.
	transaction_total_value numeric(12, 2) NOT NULL,	-- Total purchase amount in BRL.
	transaction_purchase_date date NOT NULL,	-- Date the purchase was made.
	transaction_item_receipt_date date NULL,	-- Date the physical items were received (when different from purchase date).
	transaction_receipt_deadline_days smallint NOT NULL   DEFAULT 30,	-- Expected number of days after purchase for points to be credited.
	transaction_points_per_real smallint NOT NULL,	-- Points earned per BRL spent on this transaction.
	transaction_status smallint NOT NULL   DEFAULT 0,	-- Transaction status.
		-- 0 = Pending (awaiting point credit).
		-- 1 = Received (points credited successfully).
		-- 2 = Disputed (user flagged as not received after deadline).
		-- 3 = Late (deadline passed without credit, pending user action).
	transaction_status_updated_at timestamp NULL,	-- Timestamp of the last status change.
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public.transaction
	ADD CONSTRAINT transaction_fk1 FOREIGN KEY (user_id)
	REFERENCES public."user" (user_id)
;

ALTER TABLE public.transaction
	ADD CONSTRAINT transaction_fk2 FOREIGN KEY (loyalty_program_id)
	REFERENCES public.loyalty_program (loyalty_program_id)
;

ALTER TABLE public.transaction
	ADD CONSTRAINT transaction_chk1 CHECK (
		transaction_status IN (0, 1, 2, 3)
	)
;

ALTER TABLE public.transaction
	ADD CONSTRAINT transaction_chk2 CHECK (
		transaction_total_value > 0
	)
;

ALTER TABLE public.transaction
	ADD CONSTRAINT transaction_chk3 CHECK (
		transaction_points_per_real > 0
	)
;

ALTER TABLE public.transaction
	ADD CONSTRAINT transaction_chk4 CHECK (
		transaction_receipt_deadline_days > 0
	)
;

CREATE INDEX transaction_user_idx ON public.transaction (user_id)
;

CREATE INDEX transaction_user_status_idx ON public.transaction (user_id, transaction_status)
;

CREATE INDEX transaction_purchase_date_idx ON public.transaction (transaction_purchase_date)
;

CREATE TRIGGER transaction_upd_trigger BEFORE UPDATE
    ON public.transaction FOR EACH ROW EXECUTE PROCEDURE
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.transaction
	IS 'User purchase transactions with associated loyalty point expectations. Tracks pending, received, and disputed point credits.'
;

COMMENT ON COLUMN public.transaction.transaction_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public.transaction.user_id
	IS 'FK to user. Owner of the transaction.'
;

COMMENT ON COLUMN public.transaction.loyalty_program_id
	IS 'FK to loyalty_program. Points program tied to this transaction.'
;

COMMENT ON COLUMN public.transaction.transaction_description
	IS 'Description of the purchased item. Ex: TV.'
;

COMMENT ON COLUMN public.transaction.transaction_store
	IS 'Name of the store or retailer. Ex: Casas Bahia.'
;

COMMENT ON COLUMN public.transaction.transaction_total_value
	IS 'Total purchase amount in BRL.'
;

COMMENT ON COLUMN public.transaction.transaction_purchase_date
	IS 'Date the purchase was made.'
;

COMMENT ON COLUMN public.transaction.transaction_item_receipt_date
	IS 'Date the physical items were received (when different from purchase date).'
;

COMMENT ON COLUMN public.transaction.transaction_receipt_deadline_days
	IS 'Expected number of days after purchase for points to be credited.'
;

COMMENT ON COLUMN public.transaction.transaction_points_per_real
	IS 'Points earned per BRL spent on this transaction.'
;

COMMENT ON COLUMN public.transaction.transaction_status
	IS 'Transaction status.
		0 = Pending (awaiting point credit).
		1 = Received (points credited successfully).
		2 = Disputed (user flagged as not received after deadline).
		3 = Late (deadline passed without credit, pending user action).'
;

COMMENT ON COLUMN public.transaction.transaction_status_updated_at
	IS 'Timestamp of the last status change.'
;

COMMENT ON COLUMN public.transaction.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.transaction.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.transaction.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.transaction.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.transaction.row_is_deleted
	IS 'Row has been removed.'
;
