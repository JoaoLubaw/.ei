/* Create Functions */

CREATE OR REPLACE FUNCTION public.update_row_update_time() 
RETURNS TRIGGER AS 
$$
BEGIN
    NEW.row_update_time := NOW();
    RETURN NEW;
END;
 $$ LANGUAGE 'plpgsql'
;

COMMENT ON FUNCTION public.update_row_update_time()
	IS 'General trigger function to update row metadata.'
;