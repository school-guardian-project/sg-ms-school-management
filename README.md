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

Las migraciones de `database/ms-school-db/01-ddl/04-alter` mantienen las columnas
de ubicacion y crean una sede central en colegios activos que no tienen ninguna
sede, sin duplicar ni modificar las sedes existentes.

La relacion administrativa ya existe en `School.SchoolAdmin` y es mantenida por IAM.
Los demas usuarios se relacionan a la escuela mediante `Iam.Profile.CampuseId`
y `School.SchoolCampus.SchoolId`.