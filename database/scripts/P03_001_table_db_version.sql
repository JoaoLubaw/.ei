/* Drop Sequences for Autonumber Columns */
DROP SEQUENCE IF EXISTS public.db_version_id_seq
;

/* Drop Tables */
DROP TABLE IF EXISTS public.db_version CASCADE
;

/* Create Tables */
CREATE TABLE public.db_version
(
	version_id integer NOT NULL   DEFAULT NEXTVAL(('db_version_id_seq'::text)::regclass),	-- Table primary key.
	
	version_number varchar(256) NOT NULL,	-- Version number. Ex. 1.0.5
	version_notes varchar(256) NULL,	-- Version description and notes.

	-- Audit columns
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(256) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(256) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */
ALTER TABLE public.db_version ADD CONSTRAINT db_version_pk
	PRIMARY KEY (version_id)
;

ALTER TABLE public.db_version 
  ADD CONSTRAINT db_version_uk1 UNIQUE (version_number)
;

CREATE TRIGGER db_version_upd_trigger BEFORE UPDATE
    ON db_version FOR EACH ROW EXECUTE PROCEDURE 
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.db_version
	IS 'Database metadata version table.'
;

COMMENT ON COLUMN public.db_version.version_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public.db_version.version_number
	IS 'Version number. Ex. 1.0.5'
;

COMMENT ON COLUMN public.db_version.version_notes
	IS 'Version description and notes.'
;

COMMENT ON COLUMN public.db_version.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.db_version.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.db_version.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.db_version.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.db_version.row_is_deleted
	IS 'Row has been removed.'
;

CREATE SEQUENCE public.db_version_id_seq INCREMENT 1 START 1
;