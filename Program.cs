using CrudClientesCSharp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

var conexion = builder.Configuration.GetConnectionString("ClientesDb")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:ClientesDb en appsettings.Local.json o en una variable de entorno.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(conexion, new MariaDbServerVersion(new Version(10, 4, 32))));
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStatusCodePages("text/plain; charset=utf-8", "Solicitud no disponible. Código HTTP: {0}.");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Clientes}/{action=Index}/{id?}");
app.Run();
