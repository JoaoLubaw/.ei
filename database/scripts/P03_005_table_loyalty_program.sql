/* Drop Sequences for Autonumber Columns */

DROP SEQUENCE IF EXISTS public.loyalty_program_id_seq
;

/* Drop Tables */

DROP TABLE IF EXISTS public.loyalty_program CASCADE
;

/* Create Tables */

CREATE TABLE public.loyalty_program
(
	loyalty_program_id integer NOT NULL   DEFAULT NEXTVAL(('loyalty_program_id_seq'::text)::regclass),	-- Table primary key.
	loyalty_program_name varchar(100) NOT NULL,	-- Program display name. Ex: Livelo, Smiles.
	loyalty_program_logo_url varchar(512) NULL,	-- URL to the program logo asset.
	loyalty_program_brand_primary_color varchar(7) NULL,	-- Primary brand color in hex format. Ex: #FF0066.
	loyalty_program_brand_secondary_color varchar(7) NULL,	-- Secondary brand color in hex format. Ex: #00FF66.
	loyalty_program_is_active boolean NOT NULL   DEFAULT True,	-- Whether this program is currently available for selection.
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public.loyalty_program ADD CONSTRAINT loyalty_program_pk
	PRIMARY KEY (loyalty_program_id)
;

ALTER TABLE public.loyalty_program
	ADD CONSTRAINT loyalty_program_uk1 UNIQUE (loyalty_program_name)
;

CREATE TRIGGER loyalty_program_upd_trigger BEFORE UPDATE
    ON public.loyalty_program FOR EACH ROW EXECUTE PROCEDURE
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.loyalty_program
	IS 'Catalog of supported loyalty point programs. Managed as seed data.'
;

COMMENT ON COLUMN public.loyalty_program.loyalty_program_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public.loyalty_program.loyalty_program_name
	IS 'Program display name. Ex: Livelo, Smiles.'
;

COMMENT ON COLUMN public.loyalty_program.loyalty_program_logo_url
	IS 'URL to the program logo asset.'
;

COMMENT ON COLUMN public.loyalty_program.loyalty_program_brand_primary_color
	IS 'Primary brand color in hex format. Ex: #FF0066.'
;

COMMENT ON COLUMN public.loyalty_program.loyalty_program_brand_secondary_color
	IS 'Secondary brand color in hex format. Ex: #00FF66.'
;

COMMENT ON COLUMN public.loyalty_program.loyalty_program_is_active
	IS 'Whether this program is currently available for selection.'
;

COMMENT ON COLUMN public.loyalty_program.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.loyalty_program.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.loyalty_program.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.loyalty_program.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.loyalty_program.row_is_deleted
	IS 'Row has been removed.'
;

CREATE SEQUENCE public.loyalty_program_id_seq INCREMENT 1 START 1
;