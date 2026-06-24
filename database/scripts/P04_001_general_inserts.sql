-- General inserts for the database.

--- Insert the loyalty programs into the database.
INSERT INTO public.loyalty_program (loyalty_program_name, loyalty_program_brand_primary_color, loyalty_program_brand_secondary_color) VALUES
    ('Esfera',         '#CC0000', '#000000'),
    ('Dotz',          '#000000', '#FF4F0D'),
    ('Livelo',        '#FF0066', '#151F4F'),
    ('Inter Loop',    '#EA7100', '#FFFFFF'),
    ('XP Investimentos', '#000000', '#FFFFFF'),
    ('Átomos',        '#2D2D2D', '#FFFFFF'),
    ('Smiles',        '#EB7F02', '#FFFFFF'),
    ('Latam Pass',    '#1B0088', '#ED1550'),
    ('Azul Fidelidade',      '#18B4E9', '#5061AA'),
    ('Itaú',          '#18B4E9', '#FFFFFF'),
    ('Caixa',         '#015CA9', '#F59700'),
    ('Stix',          '#FFFFFF', '#9D45E8'),
    ('Outro',         '#888888', '#FFFFFF')
;


-- Versions insertions.
INSERT INTO public.db_version (version_number, version_notes) VALUES 
    ('1.0.0', 'First database version.')
;
