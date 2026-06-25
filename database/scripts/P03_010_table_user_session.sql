/* Drop Tables */
DROP TABLE IF EXISTS public.user_session CASCADE;


CREATE TABLE public.user_session
(
    user_session_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    
    user_session_refresh_token varchar(255) NOT NULL,
    user_session_refresh_token_expires_at timestamp NOT NULL,
    
    user_session_device_info varchar(255) NULL, -- Optional: 'iPhone 13', 'Chrome on Windows', etc.
    user_session_ip_address varchar(45) NULL,   -- IPv4 or IPv6
    
    user_session_is_revoked boolean NOT NULL DEFAULT False, -- Allows forced logout

    user_session_push_notification_token varchar(255) NULL, -- Optional: Token for push notifications (e.g., Firebase Cloud Messaging)
    
	-- Audit columns
	row_creation_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row creation time.
	row_update_time timestamp NOT NULL   DEFAULT CURRENT_TIMESTAMP,	-- Row last update time.
	row_creation_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that inserted row.
	row_update_user varchar(30) NOT NULL   DEFAULT 'system',	-- The user that last updated row.
	row_is_deleted boolean NOT NULL   DEFAULT False	-- Row has been removed.
);

ALTER TABLE public.user_session 
    ADD CONSTRAINT user_session_fk1 
        FOREIGN KEY (user_id) 
        REFERENCES public."user" (user_id) 
    ON DELETE CASCADE
;

ALTER TABLE public.user_session 
    ADD CONSTRAINT user_session_uk1 
        UNIQUE (user_session_refresh_token)
    ;

CREATE INDEX user_session_user_idx 
    ON public.user_session (user_id)
;