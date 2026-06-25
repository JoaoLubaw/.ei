/* Drop Sequences for Autonumber Columns */

DROP SEQUENCE IF EXISTS public.configuration_id_seq
;

/* Drop Tables */

DROP TABLE IF EXISTS public.configuration CASCADE
;

/* Create Tables */

CREATE TABLE public.configuration
(
	configuration_id integer NOT NULL   DEFAULT NEXTVAL(('configuration_id_seq'::text)::regclass),
	configuration_name varchar(50) NOT NULL UNIQUE,
	configuration_description varchar(256) NULL,
	configuration_value varchar(256) NOT NULL,
	configuration_type smallint NOT NULL,

	-- Audit columns
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public.configuration ADD CONSTRAINT configuration_pk
	PRIMARY KEY (configuration_id)
;

CREATE TRIGGER configuration_upd_trigger BEFORE UPDATE
    ON public.configuration FOR EACH ROW EXECUTE PROCEDURE 
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.configuration
	IS 'Configuration table'
;

COMMENT ON COLUMN public.configuration.configuration_name
	IS 'Configuration name'
;

COMMENT ON COLUMN public.configuration.configuration_description
	IS 'Configuration description'
;

COMMENT ON COLUMN public.configuration.configuration_value
	IS 'Configuration current value'
;

COMMENT ON COLUMN public.configuration.configuration_type
	IS 'Configuration type: 
			Text = 0,
			Number = 1,
			Decimal = 2,
			Boolean = 3,
			Date = 4,
			URL = 5
	)'
;

COMMENT ON COLUMN public.configuration.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.configuration.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.configuration.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.configuration.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.configuration.row_is_deleted
	IS 'Row has been removed.'
;

CREATE SEQUENCE public.configuration_id_seq INCREMENT 1 START 1
;