/* Drop Tables */

DROP TABLE IF EXISTS public.transaction_media CASCADE
;

/* Create Tables */

CREATE TABLE public.transaction_media
(
	transaction_media_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),	-- Table primary key.

	transaction_id UUID NOT NULL,	-- FK to transaction. The transaction this media is attached to.

	transaction_media_file_url varchar(512) NOT NULL,	-- Storage URL of the uploaded file (image or PDF).
	transaction_media_file_type smallint NOT NULL,	-- Type of the uploaded file. 0 = Image. 1 = PDF.
	transaction_media_display_order smallint NOT NULL   DEFAULT 0,	-- Display order within the transaction media gallery.

	-- Audit columns
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(256) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(256) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public.transaction_media
	ADD CONSTRAINT transaction_media_fk1 FOREIGN KEY (transaction_id)
	REFERENCES public.transaction (transaction_id)
;

ALTER TABLE public.transaction_media
	ADD CONSTRAINT transaction_media_chk1 CHECK (
		transaction_media_file_type IN (0, 1)
	)
;

CREATE INDEX transaction_media_transaction_idx ON public.transaction_media (transaction_id)
;

CREATE TRIGGER transaction_media_upd_trigger BEFORE UPDATE
    ON public.transaction_media FOR EACH ROW EXECUTE PROCEDURE
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.transaction_media
	IS 'Proof files (screenshots, receipts, NFe) attached by the user to a transaction. Used to support disputes or manual point requests.'
;

COMMENT ON COLUMN public.transaction_media.transaction_media_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public.transaction_media.transaction_id
	IS 'FK to transaction. The transaction this media is attached to.'
;

COMMENT ON COLUMN public.transaction_media.transaction_media_file_url
	IS 'Storage URL of the uploaded file (image or PDF).'
;

COMMENT ON COLUMN public.transaction_media.transaction_media_file_type
	IS 'Type of the uploaded file.
		0 = Image.
		1 = PDF.'
;

COMMENT ON COLUMN public.transaction_media.transaction_media_display_order
	IS 'Display order within the transaction media gallery.'
;

COMMENT ON COLUMN public.transaction_media.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.transaction_media.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.transaction_media.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.transaction_media.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.transaction_media.row_is_deleted
	IS 'Row has been removed.'
;
