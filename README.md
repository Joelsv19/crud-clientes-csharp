# CRUD de clientes en C#

Tarea 1 de la Semana 3 de Aplicaciones Web, sexto nivel de Ingeniería en Software de UNIANDES. Docente: Ing. Luis Llerena.

Aplicación para registrar, listar, consultar, editar y eliminar clientes con confirmación. La tabla tiene un ID autoincremental y diez campos: nombres, apellidos, cédula, correo, teléfono, dirección, fecha de nacimiento, ocupación, provincia y cantón.

## Tecnologías

C#, ASP.NET Core MVC (.NET 9), Entity Framework Core 9.0.13, Pomelo 9.0.0, MariaDB, Razor, CSS y jQuery. Se utilizó .NET 9 porque .NET 10 no estaba instalado en el equipo.

## Ejecutar

1. Descargar el proyecto y abrir una terminal en la carpeta que contiene `CrudClientesCSharp.csproj`.
2. Iniciar MySQL/MariaDB desde XAMPP. El proyecto está configurado para MariaDB 10.4.32 en el puerto 3306; Apache no es necesario.
3. Copiar el archivo de ejemplo:

   ```powershell
   Copy-Item appsettings.Local.example.json appsettings.Local.json
   ```

   Completar el usuario y la contraseña locales en `appsettings.Local.json`. Ese archivo está excluido de Git. El usuario debe tener permisos para crear la base independiente `crud_clientes_csharp_semana3` y aplicar las migraciones.

4. Ejecutar:

   ```powershell
   dotnet restore
   dotnet tool restore
   dotnet tool run dotnet-ef database update
   dotnet build
   dotnet run --launch-profile http
   ```

5. Abrir [http://localhost:5183](http://localhost:5183). Para detener la aplicación, pulsar `Ctrl+C`.

La aplicación usa Kestrel, el servidor de ASP.NET Core; no se coloca dentro de `htdocs`. La herramienta EF está fijada a 9.0.13 en `.config/dotnet-tools.json`.

## Organización

- `Models`: datos y validaciones de cliente, provincia y cantón.
- `Data`: contexto de Entity Framework y relaciones entre tablas.
- `Controllers`: operaciones del CRUD y consulta de cantones por provincia.
- `Views`: vistas Razor del listado, formularios, detalles y eliminación.
- `Migrations`: creación de las tablas y datos iniciales de ubicación.
- `wwwroot`: CSS y JavaScript.

Las migraciones incluyen 4 provincias y 12 cantones de Ecuador como datos de demostración. No es el catálogo nacional completo. Al instalar, la tabla de clientes comienza vacía. La selección de cantón depende de la provincia y también se valida en el servidor.

Se tomó como referencia la estructura de [Factura y Provincias del docente](https://github.com/lleroc/Sexto_om27).

Con el servidor iniciado, se pueden repetir las 56 comprobaciones del CRUD con Python: `python Pruebas/verificar_crud.py`. El script crea y elimina únicamente sus propios clientes ficticios.
