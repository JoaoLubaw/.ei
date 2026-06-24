\set ON_ERROR_STOP on

-- Extension to generate random UUIDs
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ===== User creation =====
-- :'pontuei_password' is a psql variable — only works outside DO $$ blocks.

SELECT NOT EXISTS (
    SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'pontuei'
) AS should_create_user \gset

\if :should_create_user
    CREATE USER pontuei WITH LOGIN PASSWORD :'pontuei_password';
    \echo 'NOTICE: User "pontuei" created.'
\else
    \echo 'NOTICE: User "pontuei" already exists. Skipping creation.'
\endif

-- ===== Database creation =====

SELECT NOT EXISTS (
    SELECT 1 FROM pg_database WHERE datname = 'pontuei'
) AS should_create_db \gset

\if :should_create_db
    CREATE DATABASE pontuei WITH OWNER = pontuei;
    \echo 'NOTICE: Database "pontuei" created.'
\else
    \echo 'NOTICE: Database "pontuei" already exists. Skipping.'
\endif

GRANT ALL PRIVILEGES ON DATABASE pontuei TO pontuei;

-- ===== Schema public permissions =====
-- PostgreSQL 15+ revoked CREATE on public schema from PUBLIC by default.
-- Must explicitly grant it to the application user after connecting to the target database.

\c pontuei

GRANT ALL ON SCHEMA public TO pontuei;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO pontuei;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO pontuei;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO pontuei;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO pontuei;