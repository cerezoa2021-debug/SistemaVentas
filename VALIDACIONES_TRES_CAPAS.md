# Validaciones de datos en tres capas

## Alcance

Las reglas están aplicadas en los formularios, los modelos/controladores del servidor y el esquema SQLite. Las validaciones del navegador mejoran la experiencia, pero no sustituyen las del servidor ni las de la base de datos.

## Reglas principales

| Dato | Regla |
|---|---|
| Categoría | Nombre obligatorio, entre 1 y 80 caracteres; letras españolas, espacios, puntos, apóstrofes y guiones; sin números |
| Producto | Nombre obligatorio, entre 1 y 100 caracteres; al menos una letra, solo letras españolas, números y `. , % ( ) / -`; precio entre 0 y 99,999,999.99; stock y stock mínimo entre 0 y 1,000,000; categoría existente |
| Cliente | Nombre obligatorio, entre 1 y 120 caracteres; letras españolas, espacios, puntos, apóstrofes y guiones; correo válido de hasta 254 caracteres; celular opcional de 7 u 8 dígitos |
| Detalle de venta | Cantidad entre 1 y 1,000,000; producto válido; precio y subtotal calculados y validados en el servidor |
| Venta | Cliente existente; montos entre 0 y 99,999,999.99; el monto recibido debe cubrir el total; debe incluir productos con stock suficiente |

El precio cero sigue permitido para conservar el comportamiento previo del sistema.

## Regla x Capa

| Regla | Frontend | Backend | BD |
|---|---|---|---|
| Categoría: nombre obligatorio, 1-80 caracteres, formato de letras y único | Regex unobtrusive y filtrado de números en el formulario | DataAnnotations; MVC muestra error de ModelState; API responde 409 si el nombre ya existe | CHECK de longitud y sin dígitos; índice UNIQUE en `Categorias.Nombre` |
| Producto: nombre 1-100 caracteres, al menos una letra y formato permitido; precio 0-99,999,999.99, stock y mínimo 0-1,000,000; categoría existente | Validación HTML/unobtrusive | DataAnnotations y control de categoría existente | CHECK de longitud y rangos; FK Producto-Categoria con RESTRICT |
| Cliente: nombre 1-120 caracteres y formato de letras; correo válido hasta 254; celular opcional de 7-8 dígitos | Regex unobtrusive; nombres filtran dígitos y celular filtra caracteres no numéricos | DataAnnotations y validación del servidor | CHECK de longitud y sin dígitos en nombre; celular nulo o de 7-8 dígitos |
| Detalle: cantidad 1-1,000,000, producto válido; precio/subtotal calculados en servidor | Formulario limita cantidad según stock | Ventas validan producto, cantidad y stock; precio y subtotal no se confían al cliente | CHECK de cantidad y montos; FK DetalleVenta-Producto con RESTRICT |
| Venta: cliente existente, montos válidos, recibido cubre total y stock suficiente | Validación del formulario de venta | El servidor recalcula importes y comprueba cliente, productos y stock; la API descarta claves/navegaciones de entidad recibidas | CHECK de montos; FK Venta-Cliente con RESTRICT |

Las validaciones del navegador mejoran la experiencia, pero no sustituyen las del servidor ni las de la base de datos. Las regex compartidas se definen en `Models/Reglas.cs`. El esquema se configura en `Data/AppDbContext.cs`; las migraciones `ValidacionesTresCapas`, `CategoriaUnicaYRestriccionFK` y `ValidacionesFormato` incorporan las restricciones.

## Pruebas

### Navegador

1. En **Productos**, intenta guardar un nombre vacío o de más de 100 caracteres, precio negativo, stock negativo o superior a 1,000,000. El navegador debe impedir valores inválidos. Guarda también un producto válido con precio cero.
2. En **Clientes**, intenta guardar nombre vacío, correo mal formado o valores que excedan los límites.
3. En **Clientes** y **Categorías**, prueba nombres con dígitos: el formulario los filtra al escribir. Prueba también símbolos no admitidos para comprobar el mensaje de formato.
4. En **Clientes**, el celular debe filtrar letras y admitir solo 7 u 8 dígitos; prueba valores de 6 y 9 dígitos para ver el error.
5. En **Productos**, prueba un nombre sin letras (por ejemplo `123`) y nombres con caracteres no permitidos para comprobar el error de formato.
6. En **Categorías**, intenta nombres vacíos y de más de 80 caracteres; ambos deben mostrar validación. Crea una categoría válida y vuelve a intentar crear otra con el mismo nombre: debe aparecer un error junto al nombre. Repite en edición usando el nombre de otra categoría existente.
7. En **Ventas**, intenta continuar sin cliente, sin productos, con cantidad cero, con más unidades que el stock disponible y con recibido inferior al total. La venta no debe guardarse.

### Thunder Client

1. `POST /api/categorias` con `{"nombre":"Categoria API única"}` debe responder 201. Repite el POST con el mismo nombre: debe responder 409 con un mensaje amable.
2. `PUT /api/categorias/{id}` usando el nombre de otra categoría debe responder 409; un nombre disponible debe responder 204.
3. `POST /api/ventas` con una venta válida debe crearla y actualizar stock. Incluye en el JSON valores falsos para `id`, `cliente` y, en cada detalle, `id`, `venta` y `producto`: se deben ignorar y no deben crear ni alterar entidades relacionadas; el cliente/producto se resuelven por sus IDs.
4. Intenta `POST /api/ventas` con cliente o producto inexistente, sin detalles, cantidad no positiva, stock insuficiente y recibido menor al total: la API debe rechazar cada solicitud y no registrar la venta ni alterar stock.
5. Intenta eliminar por API una categoría con productos, un producto con detalles de venta y un cliente con ventas: el borrado debe rechazarse por las relaciones RESTRICT.

La migración/script se genera sin ejecutar `database update`; estas pruebas se ejecutan después de aplicar la migración al entorno de pruebas.

## Aplicar la migración

Ejecuta en la raíz del proyecto:

```powershell
dotnet ef database update
```

La migración debe aplicarse a cada base de datos del entorno antes de depender de las nuevas restricciones SQLite.

El proveedor SQLite de EF Core no admite `dotnet ef migrations script --idempotent`. `MigracionValidacionesFormato.sql` es un script no idempotente delimitado entre `CategoriaUnicaYRestriccionFK` y `ValidacionesFormato`; verifica el historial de la base antes de ejecutarlo.
