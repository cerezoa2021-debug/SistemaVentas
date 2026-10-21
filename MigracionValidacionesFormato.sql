BEGIN TRANSACTION;
CREATE TABLE "ef_temp_Clientes" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Clientes" PRIMARY KEY AUTOINCREMENT,
    "Celular" TEXT NULL,
    "Email" TEXT NULL,
    "Nombre" TEXT NOT NULL,
    CONSTRAINT "CK_Clientes_Celular_Length" CHECK ("Celular" IS NULL OR ("Celular" NOT GLOB '*[^0-9]*' AND length("Celular") BETWEEN 7 AND 8)),
    CONSTRAINT "CK_Clientes_Email_Length" CHECK ("Email" IS NULL OR length("Email") <= 254),
    CONSTRAINT "CK_Clientes_Nombre_Format" CHECK ("Nombre" NOT GLOB '*[0-9]*'),
    CONSTRAINT "CK_Clientes_Nombre_Length" CHECK (length(trim("Nombre")) BETWEEN 1 AND 120)
);

INSERT INTO "ef_temp_Clientes" ("Id", "Celular", "Email", "Nombre")
SELECT "Id", "Celular", "Email", "Nombre"
FROM "Clientes";

CREATE TABLE "ef_temp_Categorias" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Categorias" PRIMARY KEY AUTOINCREMENT,
    "Nombre" TEXT NOT NULL,
    CONSTRAINT "CK_Categorias_Nombre_Format" CHECK ("Nombre" NOT GLOB '*[0-9]*'),
    CONSTRAINT "CK_Categorias_Nombre_Length" CHECK (length(trim("Nombre")) BETWEEN 1 AND 80)
);

INSERT INTO "ef_temp_Categorias" ("Id", "Nombre")
SELECT "Id", "Nombre"
FROM "Categorias";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Clientes";

ALTER TABLE "ef_temp_Clientes" RENAME TO "Clientes";

DROP TABLE "Categorias";

ALTER TABLE "ef_temp_Categorias" RENAME TO "Categorias";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_Categorias_Nombre" ON "Categorias" ("Nombre");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261021210147_ValidacionesFormato', '10.0.12');

