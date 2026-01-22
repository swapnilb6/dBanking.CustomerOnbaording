DO statement$$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'postgres1') THEN
      CREATE ROLE postgres1 LOGIN PASSWORD 'postgres123' SUPERUSER
   END IF
ENDstatement$$;

DO statement2$$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'dBanking_CMS') THEN
      CREATE DATABASE "dBanking_CMS" OWNER postgres1;
   END IF
END$$;
