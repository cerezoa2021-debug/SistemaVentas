BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_Categorias_Nombre" ON "Categorias" ("Nombre");

CREATE TABLE "ef_temp_DetallesVenta" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DetallesVenta" PRIMARY KEY AUTOINCREMENT,
    "Cantidad" INTEGER NOT NULL,
    "Precio" TEXT NOT NULL,
    "ProductoId" INTEGER NOT NULL,
    "Subtotal" TEXT NOT NULL,
    "VentaId" INTEGER NOT NULL,
    CONSTRAINT "CK_DetallesVenta_Cantidad_Range" CHECK ("Cantidad" BETWEEN 1 AND 1000000),
    CONSTRAINT "CK_DetallesVenta_Precio_Range" CHECK (CAST("Precio" AS NUMERIC) BETWEEN 0 AND 99999999.99),
    CONSTRAINT "CK_DetallesVenta_Subtotal_Range" CHECK (CAST("Subtotal" AS NUMERIC) BETWEEN 0 AND 99999999.99),
    CONSTRAINT "FK_DetallesVenta_Productos_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Productos" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_DetallesVenta_Ventas_VentaId" FOREIGN KEY ("VentaId") REFERENCES "Ventas" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_DetallesVenta" ("Id", "Cantidad", "Precio", "ProductoId", "Subtotal", "VentaId")
SELECT "Id", "Cantidad", "Precio", "ProductoId", "Subtotal", "VentaId"
FROM "DetallesVenta";

CREATE TABLE "ef_temp_Productos" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Productos" PRIMARY KEY AUTOINCREMENT,
    "CategoriaId" INTEGER NOT NULL,
    "Nombre" TEXT NOT NULL,
    "Precio" TEXT NOT NULL,
    "Stock" INTEGER NOT NULL,
    "StockMinimo" INTEGER NOT NULL,
    CONSTRAINT "CK_Productos_Nombre_Length" CHECK (length(trim("Nombre")) BETWEEN 1 AND 100),
    CONSTRAINT "CK_Productos_Precio_Range" CHECK (CAST("Precio" AS NUMERIC) BETWEEN 0 AND 99999999.99),
    CONSTRAINT "CK_Productos_Stock_Range" CHECK ("Stock" BETWEEN 0 AND 1000000),
    CONSTRAINT "CK_Productos_StockMinimo_Range" CHECK ("StockMinimo" BETWEEN 0 AND 1000000),
    CONSTRAINT "FK_Productos_Categorias_CategoriaId" FOREIGN KEY ("CategoriaId") REFERENCES "Categorias" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Productos" ("Id", "CategoriaId", "Nombre", "Precio", "Stock", "StockMinimo")
SELECT "Id", "CategoriaId", "Nombre", "Precio", "Stock", "StockMinimo"
FROM "Productos";

CREATE TABLE "ef_temp_Ventas" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Ventas" PRIMARY KEY AUTOINCREMENT,
    "Anulada" INTEGER NOT NULL,
    "Cambio" TEXT NOT NULL,
    "ClienteId" INTEGER NOT NULL,
    "Fecha" TEXT NOT NULL,
    "Recibido" TEXT NOT NULL,
    "Total" TEXT NOT NULL,
    CONSTRAINT "CK_Ventas_Cambio_Range" CHECK (CAST("Cambio" AS NUMERIC) BETWEEN 0 AND 99999999.99),
    CONSTRAINT "CK_Ventas_Recibido_Range" CHECK (CAST("Recibido" AS NUMERIC) BETWEEN 0 AND 99999999.99),
    CONSTRAINT "CK_Ventas_Total_Range" CHECK (CAST("Total" AS NUMERIC) BETWEEN 0 AND 99999999.99),
    CONSTRAINT "FK_Ventas_Clientes_ClienteId" FOREIGN KEY ("ClienteId") REFERENCES "Clientes" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Ventas" ("Id", "Anulada", "Cambio", "ClienteId", "Fecha", "Recibido", "Total")
SELECT "Id", "Anulada", "Cambio", "ClienteId", "Fecha", "Recibido", "Total"
FROM "Ventas";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "DetallesVenta";

ALTER TABLE "ef_temp_DetallesVenta" RENAME TO "DetallesVenta";

DROP TABLE "Productos";

ALTER TABLE "ef_temp_Productos" RENAME TO "Productos";

DROP TABLE "Ventas";

ALTER TABLE "ef_temp_Ventas" RENAME TO "Ventas";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_DetallesVenta_ProductoId" ON "DetallesVenta" ("ProductoId");

CREATE INDEX "IX_DetallesVenta_VentaId" ON "DetallesVenta" ("VentaId");

CREATE INDEX "IX_Productos_CategoriaId" ON "Productos" ("CategoriaId");

CREATE INDEX "IX_Ventas_ClienteId" ON "Ventas" ("ClienteId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261021204623_CategoriaUnicaYRestriccionFK', '10.0.12');

