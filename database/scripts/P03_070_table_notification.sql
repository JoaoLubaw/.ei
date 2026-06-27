/* Drop Tables */

DROP TABLE IF EXISTS public.notification CASCADE
;

/* Create Tables */

CREATE TABLE public.notification
(
	notification_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),	-- Table primary key.

	user_id UUID NOT NULL,	-- FK to user. Recipient of the notification.
	transaction_id UUID NULL,	-- FK to transaction. The related transaction, if applicable.

	loyalty_program_id integer NULL,	-- FK to loyalty_program. The related program, if applicable.
	notification_message varchar(512) NOT NULL,	-- Notification body text displayed to the user.
	notification_points_amount integer NULL,	-- Point amount referenced in the notification, if applicable.
	
	notification_is_read boolean NOT NULL   DEFAULT False,	-- Whether the user has read this notification.
	notification_read_at timestamp NULL,	-- Timestamp when the notification was marked as read.
	
	-- Audit columns
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(256) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(256) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public.notification
	ADD CONSTRAINT notification_fk1 FOREIGN KEY (user_id)
	REFERENCES public."user" (user_id)
;

ALTER TABLE public.notification
	ADD CONSTRAINT notification_fk2 FOREIGN KEY (transaction_id)
	REFERENCES public.transaction (transaction_id)
;

ALTER TABLE public.notification
	ADD CONSTRAINT notification_fk3 FOREIGN KEY (loyalty_program_id)
	REFERENCES public.loyalty_program (loyalty_program_id)
;

CREATE INDEX notification_user_idx ON public.notification (user_id)
;

CREATE INDEX notification_user_unread_idx ON public.notification (user_id, notification_is_read)
	WHERE notification_is_read = False
;

CREATE TRIGGER notification_upd_trigger BEFORE UPDATE
    ON public.notification FOR EACH ROW EXECUTE PROCEDURE
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.notification
	IS 'In-app notifications sent to users. Typically triggered by transaction deadline expiration or point credit events.'
;

COMMENT ON COLUMN public.notification.notification_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public.notification.user_id
	IS 'FK to user. Recipient of the notification.'
;

COMMENT ON COLUMN public.notification.transaction_id
	IS 'FK to transaction. The related transaction, if applicable.'
;

COMMENT ON COLUMN public.notification.loyalty_program_id
	IS 'FK to loyalty_program. The related program, if applicable.'
;

COMMENT ON COLUMN public.notification.notification_message
	IS 'Notification body text displayed to the user.'
;

COMMENT ON COLUMN public.notification.notification_points_amount
	IS 'Point amount referenced in the notification, if applicable.'
;

COMMENT ON COLUMN public.notification.notification_is_read
	IS 'Whether the user has read this notification.'
;

COMMENT ON COLUMN public.notification.notification_read_at
	IS 'Timestamp when the notification was marked as read.'
;

COMMENT ON COLUMN public.notification.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.notification.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.notification.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.notification.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.notification.row_is_deleted
	IS 'Row has been removed.'
;