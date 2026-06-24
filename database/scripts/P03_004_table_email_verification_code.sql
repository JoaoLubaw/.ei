/* Drop Sequences for Autonumber Columns */

DROP SEQUENCE IF EXISTS public.email_verification_code_id_seq
;

/* Drop Tables */

DROP TABLE IF EXISTS public.email_verification_code CASCADE
;

/* Create Tables */

CREATE TABLE public.email_verification_code
(
	email_verification_code_id integer NOT NULL   DEFAULT NEXTVAL(('email_verification_code_id_seq'::text)::regclass),	-- Table primary key.
	user_id UUID NOT NULL,	-- FK to user. The owner of this verification code.
	email_verification_code_code varchar(6) NOT NULL,	-- 6-digit numeric verification code sent to the user.
	email_verification_code_type smallint NOT NULL,	-- Code purpose. 0 = Email confirmation (post-registration). 1 = Password reset.
	email_verification_code_expires_at timestamp NOT NULL,	-- Timestamp after which this code is no longer valid.
	email_verification_code_used_at timestamp NULL,	-- Timestamp when the code was successfully consumed. NULL if not yet used.
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public.email_verification_code ADD CONSTRAINT email_verification_code_pk
	PRIMARY KEY (email_verification_code_id)
;

ALTER TABLE public.email_verification_code
	ADD CONSTRAINT email_verification_code_fk1 FOREIGN KEY (user_id)
	REFERENCES public."user" (user_id)
;

ALTER TABLE public.email_verification_code
	ADD CONSTRAINT email_verification_code_chk1 CHECK (
		email_verification_code_type IN (0, 1)
	)
;

CREATE INDEX email_verification_code_user_idx ON public.email_verification_code (user_id)
;

CREATE TRIGGER email_verification_code_upd_trigger BEFORE UPDATE
    ON public.email_verification_code FOR EACH ROW EXECUTE PROCEDURE
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.email_verification_code
	IS 'Short-lived 6-digit codes sent by email. Covers both post-registration email confirmation and password reset flows.'
;

COMMENT ON COLUMN public.email_verification_code.email_verification_code_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public.email_verification_code.user_id
	IS 'FK to user. The owner of this verification code.'
;

COMMENT ON COLUMN public.email_verification_code.email_verification_code_code
	IS '6-digit numeric verification code sent to the user.'
;

COMMENT ON COLUMN public.email_verification_code.email_verification_code_type
	IS 'Code purpose.
		0 = Email confirmation (post-registration).
		1 = Password reset.'
;

COMMENT ON COLUMN public.email_verification_code.email_verification_code_expires_at
	IS 'Timestamp after which this code is no longer valid.'
;

COMMENT ON COLUMN public.email_verification_code.email_verification_code_used_at
	IS 'Timestamp when the code was successfully consumed. NULL if not yet used.'
;

COMMENT ON COLUMN public.email_verification_code.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.email_verification_code.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.email_verification_code.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.email_verification_code.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.email_verification_code.row_is_deleted
	IS 'Row has been removed.'
;

CREATE SEQUENCE public.email_verification_code_id_seq INCREMENT 1 START 1
;
