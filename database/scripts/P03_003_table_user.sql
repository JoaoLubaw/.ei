/* Drop Tables */

DROP TABLE IF EXISTS public."user" CASCADE
;

/* Create Tables */

CREATE TABLE public."user"
(
	user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),	-- Table primary key.
	user_google_id varchar(128) NULL,	-- Google OAuth2 subject identifier. NULL when account uses only email/password login.
	
	user_name varchar(100) NOT NULL,	-- User display name.
	user_email varchar(254) NOT NULL,	-- User email address. Used as login identifier.
	user_phone_number varchar(20) NULL,	-- User phone number. Optional field for communication or verification purposes.
	
	user_password_hash varchar(256) NOT NULL,	-- Bcrypt hash of the user password.
	
	user_email_verified boolean NOT NULL   DEFAULT False,	-- Whether the user has confirmed their email address.
	user_email_verified_at timestamp NULL,	-- Timestamp of email confirmation.
	
	user_push_notifications_enabled boolean NOT NULL   DEFAULT True,	-- Whether the user opted in to push notifications.
	user_email_notifications_enabled boolean NOT NULL   DEFAULT True,	-- Whether the user opted in to email notifications.
		
	user_is_admin boolean NOT NULL   DEFAULT False,	-- Whether the user has admin privileges.

	user_accepted_terms boolean NOT NULL   DEFAULT True,	-- Whether the user has accepted the terms and conditions.
	user_accepted_terms_at timestamp NOT NULL,	-- Timestamp of terms and conditions acceptance.
	user_accepted_terms_version varchar(20) NOT NULL,	-- Version of the terms and conditions accepted by the user.
	
	-- Audit columns
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public."user"
	ADD CONSTRAINT user_uk1 UNIQUE (user_email)
;

ALTER TABLE public."user"
	ADD CONSTRAINT user_uk2 UNIQUE (user_google_id)
;

ALTER TABLE public."user"
	ADD CONSTRAINT user_chk1 CHECK (
		user_password_hash IS NOT NULL OR user_google_id IS NOT NULL
	)
;

CREATE INDEX user_email_idx ON public."user" (user_email)
;

CREATE TRIGGER user_upd_trigger BEFORE UPDATE
    ON public."user" FOR EACH ROW EXECUTE PROCEDURE
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public."user"
	IS 'Application user accounts. Supports email/password and Google OAuth2 login.'
;

COMMENT ON COLUMN public."user".user_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public."user".user_name
	IS 'User display name.'
;

COMMENT ON COLUMN public."user".user_email
	IS 'User email address. Used as login identifier.'
;

COMMENT ON COLUMN public."user".user_password_hash
	IS 'Bcrypt hash of the user password. NULL when account uses only social login.'
;

COMMENT ON COLUMN public."user".user_phone_number
	IS 'User phone number. Optional field for communication or verification purposes.'
;

COMMENT ON COLUMN public."user".user_google_id
	IS 'Google OAuth2 subject identifier. NULL when account uses only email/password login.'
;

COMMENT ON COLUMN public."user".user_email_verified
	IS 'Whether the user has confirmed their email address.'
;

COMMENT ON COLUMN public."user".user_email_verified_at
	IS 'Timestamp of email confirmation.'
;

COMMENT ON COLUMN public."user".user_push_notifications_enabled
	IS 'Whether the user opted in to push notifications.'
;

COMMENT ON COLUMN public."user".user_email_notifications_enabled
	IS 'Whether the user opted in to email notifications.'
;

COMMENT ON COLUMN public."user".user_is_admin
	IS 'Whether the user has admin privileges.'
;

COMMENT ON COLUMN public."user".row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public."user".row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public."user".row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public."user".row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public."user".row_is_deleted
	IS 'Row has been removed.'
;