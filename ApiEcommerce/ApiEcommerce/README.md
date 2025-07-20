# Notas Sobre este proyecto.

### Configuración de Base De Datos.

- En el _appsettings.json_ se agregó la siguiente configuración de la **Base de Datos**.

```json
"ConnectionStrings": {
  "ConexionSql": "Server=localhost,1433;Database=ApiEcommerceNET8;User ID=SA;Password=MyStrongPass123;TrustServerCertificate=true;MultipleActiveResultSets=true"
},
```

- Para enlazar la conexión de `appsettings.json`, en **Program.cs** agregar el siguiente código:

```csharp
// En ConexionSql debe de ser el mismo nombre que se agregó en el appsettings.json.
var dbConnectionString = builder.Configuration.GetConnectionString("ConexionSql");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(dbConnectionString));
builder.Services.AddControllers();
```

### Entity Framework

1. Para instalar las herramientas (manera global) de **_Entity Framework_** `dotnet tool install --global dotnet-ef`.

2. Verificar que se instaló correctamente `dotnet ef --version`.

3. Agregar nuevo paquete con NuGet `dotnet add package Microsoft.EntityFrameworkCore.Design`.

4. Ejecutar las migraciones `dotnet ef migrations add InitialMigration`. **_(InitialMigration es para asignarle el nombre a esa migración)_**

5. Para actualizar la base de datos `dotnet ef database update`.

### SQL Server

- Para correr **SQL Server** ejecutar el `docker compose up -d` para que ejecuté el contenedor de **_Docker_**. _(Nota: Es necesario tener Docker Deskop abierto antes de ejecutar el comando.)_

### Swagger

```
http://localhost:5147/swagger/index.html
```
