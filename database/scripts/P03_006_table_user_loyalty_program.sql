/* Drop Sequences for Autonumber Columns */

DROP SEQUENCE IF EXISTS public.user_loyalty_program_id_seq
;

/* Drop Tables */

DROP TABLE IF EXISTS public.user_loyalty_program CASCADE
;

/* Create Tables */

CREATE TABLE public.user_loyalty_program
(
	user_loyalty_program_id integer NOT NULL   DEFAULT NEXTVAL(('user_loyalty_program_id_seq'::text)::regclass),	-- Table primary key.
	user_id UUID NOT NULL,	-- FK to user.
	loyalty_program_id integer NOT NULL,	-- FK to loyalty_program.
	user_loyalty_program_display_order smallint NOT NULL   DEFAULT 0,	-- Visual ordering of the program card in the user wallet. Lower values appear first.
	user_loyalty_program_is_favorite boolean NOT NULL   DEFAULT False,	-- Whether this is the user preferred program (shown as primary card on home).
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
)
;

/* Create Primary Keys, Indexes, Uniques, Checks */

ALTER TABLE public.user_loyalty_program ADD CONSTRAINT user_loyalty_program_pk
	PRIMARY KEY (user_loyalty_program_id)
;

ALTER TABLE public.user_loyalty_program
	ADD CONSTRAINT user_loyalty_program_uk1 UNIQUE (user_id, loyalty_program_id)
;

ALTER TABLE public.user_loyalty_program
	ADD CONSTRAINT user_loyalty_program_fk1 FOREIGN KEY (user_id)
	REFERENCES public."user" (user_id)
;

ALTER TABLE public.user_loyalty_program
	ADD CONSTRAINT user_loyalty_program_fk2 FOREIGN KEY (loyalty_program_id)
	REFERENCES public.loyalty_program (loyalty_program_id)
;

CREATE INDEX user_loyalty_program_user_idx ON public.user_loyalty_program (user_id)
;

CREATE TRIGGER user_loyalty_program_upd_trigger BEFORE UPDATE
    ON public.user_loyalty_program FOR EACH ROW EXECUTE PROCEDURE
    UPDATE_ROW_UPDATE_TIME();
;

/* Create Table Comments, Sequences for Autonumber Columns */

COMMENT ON TABLE public.user_loyalty_program
	IS 'Many-to-many relationship between users and their selected loyalty programs. Configured during onboarding and editable via the wallet management screen.'
;

COMMENT ON COLUMN public.user_loyalty_program.user_loyalty_program_id
	IS 'Table primary key.'
;

COMMENT ON COLUMN public.user_loyalty_program.user_id
	IS 'FK to user.'
;

COMMENT ON COLUMN public.user_loyalty_program.loyalty_program_id
	IS 'FK to loyalty_program.'
;

COMMENT ON COLUMN public.user_loyalty_program.user_loyalty_program_display_order
	IS 'Visual ordering of the program card in the user wallet. Lower values appear first.'
;

COMMENT ON COLUMN public.user_loyalty_program.user_loyalty_program_is_favorite
	IS 'Whether this is the user preferred program (shown as primary card on home).'
;

COMMENT ON COLUMN public.user_loyalty_program.row_creation_time
	IS 'Row creation time.'
;

COMMENT ON COLUMN public.user_loyalty_program.row_update_time
	IS 'Row last update time.'
;

COMMENT ON COLUMN public.user_loyalty_program.row_creation_user
	IS 'The user that inserted row.'
;

COMMENT ON COLUMN public.user_loyalty_program.row_update_user
	IS 'The user that last updated row.'
;

COMMENT ON COLUMN public.user_loyalty_program.row_is_deleted
	IS 'Row has been removed.'
;

CREATE SEQUENCE public.user_loyalty_program_id_seq INCREMENT 1 START 1
;
