# sg-ms-school-management

## School and campus status management

`GET /api/v1/schools/{id}/campuses` continues to return active campuses by default.
School editing uses `?includeInactive=true` to include inactive campuses under the
same school access policy. Responses include `status` (`Active` or `Inactive`).
Campus lookup by ID and internal assignment lookups remain active-only.

`PUT /api/v1/schools/{id}/with-campuses` accepts a campus `status`, defaulting to
`Active` for older clients. Existing inactive campuses remain inactive when
submitted unchanged; explicitly submitting `Active` reactivates them. Ownership
is checked against the requested school inside the atomic update transaction.
`removedCampusIds` still deactivates explicitly removed campuses.

School reactivation uses the existing authorized school PUT with `status: Active`.
It does not reactivate other associated records automatically.

Cada nuevo colegio se guarda, en una sola transaccion, con una `Sede central`
que hereda su direccion y coordenadas. El endpoint `with-campuses` recibe solamente
sedes adicionales, que pueden ser una lista vacia. No es necesario registrarlas
para crear un colegio.
Para conservar compatibilidad con clientes anteriores, si el POST ya incluye una
sede llamada `Sede central` (sin distinguir mayusculas), se conserva esa sede
explicita y no se crea una segunda con el mismo nombre.

El controlador utiliza `SchoolWithCampusesRequestDto` y el unico caso de uso
`CreateSchoolWithCampusesService`, que recibe direcciones y coordenadas por sede.

La migracion `database/ms-school-db/01-ddl/04-alter/005-add-school-location.sql`
agrega las columnas opcionales de ubicacion a colegios y sedes cuando no existen,
sin modificar registros ni asignar coordenadas a ubicaciones existentes.
`006-widen-school-phone.sql` amplia `School.School.Phone` a `BIGINT` sin alterar
sus valores, para que coincida con el tipo `long` utilizado por el servicio.

`007-widen-school-address.sql` widens school and campus addresses to
`VARCHAR(255) NOT NULL` without truncating data. Apply it through Liquibase to
the environment database: the 255-character DTO and EF limit does not modify
existing columns. The 5-character minimum and optional `DECIMAL(10,7)`
coordinates remain unchanged; latitude and longitude must be supplied together
or both set to `null`. School POST and PUT endpoints return `400` with Problem
Details for request validation errors, including incomplete coordinate pairs.


Al actualizar con `PUT /api/v1/schools/{id}/with-campuses`, el campo opcional
`removedCampusIds` indica las sedes que se desactivan en la misma transaccion.
Se conservan sus datos y relaciones; los listados de sedes activas ya no las muestran.
Cada id debe pertenecer al colegio y no puede aparecer tambien en `campuses`.
Omitir una sede de `campuses` no la desactiva: se requiere su id explicito.
