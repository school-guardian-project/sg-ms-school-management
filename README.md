# sg-ms-school-management

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

La relacion administrativa ya existe en `School.SchoolAdmin` y es mantenida por IAM.
Los demas usuarios se relacionan a la escuela mediante `Iam.Profile.CampuseId`
y `School.SchoolCampus.SchoolId`.

Al actualizar con `PUT /api/v1/schools/{id}/with-campuses`, el campo opcional
`removedCampusIds` indica las sedes que se desactivan en la misma transaccion.
Se conservan sus datos y relaciones; los listados de sedes activas ya no las muestran.
Cada id debe pertenecer al colegio y no puede aparecer tambien en `campuses`.
Omitir una sede de `campuses` no la desactiva: se requiere su id explicito.