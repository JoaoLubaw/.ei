-- General inserts for the database.

--- Insert the loyalty programs into the database.
INSERT INTO public.loyalty_program (loyalty_program_name, loyalty_program_brand_color) VALUES
    ('Esfera',         '#E8002D'),
    ('Dotz',          '#000000'),
    ('Livelo',        '#FF0066'),
    ('Inter Loop',    '#FF7A00'),
    ('XP Investimentos', '#000000'),
    ('Átomos',        '#2D2D2D'),
    ('Smiles',        '#F5820A'),
    ('Latam Pass',    '#D91F2A'),
    ('TudoAzul',      '#0078D4'),
    ('Itaú',          '#F36F21'),
    ('Caixa',         '#0070AF'),
    ('Stix',          '#5C2D91'),
    ('Outro',         '#888888')
;


-- Versions insertions.
INSERT INTO public.db_version (version_number, version_notes) VALUES 
    ('1.0.0', 'First database version.')
;
