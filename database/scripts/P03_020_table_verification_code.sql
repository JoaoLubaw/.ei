/* Drop Sequences for Autonumber Columns */

DROP SEQUENCE IF EXISTS public.verification_code_id_seq
;

/* Drop Tables */

DROP TABLE IF EXISTS public.verification_code CASCADE
;

/* Create Tables */

CREATE TABLE public.verification_code
(
	verification_code_id UUID PRIMARY KEY DEFAULT gen_random_uuid(), -- Changed to UUID for security (harder to guess direct references)
	user_id UUID NOT NULL,	-- FK to user. The owner of this verification code.

	verification_code_hash varchar(256) NOT NULL,	-- Hash of the 6-digit numeric verification code sent to the user.
	verification_code_type smallint NOT NULL,	-- Code purpose. 0 = Email confirmation (post-registration). 1 = Password reset.

	verification_code_expires_at timestamp NOT NULL,	-- Timestamp after which this code is no longer valid.
	verification_code_used_at timestamp NULL,	-- Timestamp when the code was successfully consumed. NULL if not yet used.

	verification_code_payload varchar(256) NULL,	-- Optional: Additional data associated with the verification code. Can be used to store context or metadata.
	verification_code_failed_attempts smallint NOT NULL DEFAULT 0,	-- Count of failed attempts to use this code. Can be used for rate limiting or security purposes.
	
	-- Audit columns
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public.verification_code ADD CONSTRAINT verification_code_pk
	PRIMARY KEY (verification_code_id)
;

ALTER TABLE public.verification_code
	ADD CONSTRAINT verification_code_fk1 FOREIGN KEY (user_id)
		REFERENCES public."user" (user_id)
	ON DELETE CASCADE;
;

ALTER TABLE public.verification_code
	ADD CONSTRAINT verification_code_chk1 CHECK (
		verification_code_type IN (0, 1)
	)
;

CREATE INDEX verification_code_user_idx ON public.verification_code (user_id)
;

CREATE TRIGGER verification_code_upd_trigger BEFORE UPDATE
    ON public.verification_code FOR EACH ROW EXECUTE PROCEDURE
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.verification_code
	IS 'Short-lived 6-digit codes sent by email. Covers both post-registration email confirmation and password reset flows.'
;

COMMENT ON COLUMN public.verification_code.verification_code_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public.verification_code.user_id
	IS 'FK to user. The owner of this verification code.'
;

COMMENT ON COLUMN public.verification_code.verification_code_hash
	IS 'Hash of the 6-digit numeric verification code sent to the user.'
;

COMMENT ON COLUMN public.verification_code.verification_code_type
	IS 'Code purpose.
			0 = Email confirmation (post-registration)
			1 = Password reset.
			2 = Email change confirmation (post-registration)
			3 = Phone number confirmation (post-registration)
			4 = Phone number change confirmation (post-registration)
			5 = Two-factor authentication (2FA) code
		'
;

COMMENT ON COLUMN public.verification_code.verification_code_expires_at
	IS 'Timestamp after which this code is no longer valid.'
;

COMMENT ON COLUMN public.verification_code.verification_code_used_at
	IS 'Timestamp when the code was successfully consumed. NULL if not yet used.'
;

COMMENT ON COLUMN public.verification_code.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.verification_code.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.verification_code.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.verification_code.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.verification_code.row_is_deleted
	IS 'Row has been removed.'
;

CREATE SEQUENCE public.verification_code_id_seq INCREMENT 1 START 1
;
