using Autofac;
using Autofac.Extensions.DependencyInjection;
using CapaAplicacion;
using CapaAplicacion.Interfaces;
using CapaAplicacion.Servicios;
using CapaData.Data;
using CapaData.DTOs;
using CapaData.Entities;
using CapaData.Interfaces;
using CapaData.Repositorios;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("IdentityConnection");

// Configura el DbContext para ApplicationDbContext y agrega Identity
builder.Services.AddDbContext<DB_ContextIdentity>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

// Configura la identidad (solo una vez)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<DB_ContextIdentity>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();


builder.Services.AddDbContextFactory<DB_Context>(options =>
{
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
        sqlOptions.CommandTimeout(180);
    });
});

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Configuración de Autofac en el contenedor
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    FabricaServicios.RegisterServices(containerBuilder, builder.Configuration);
});




builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";  // Ruta correcta para el login
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";  // Ruta correcta para acceso denegado
});

builder.Services.AddTransient<IExtendedEmailSender, EmailSender>(i =>
    new EmailSender(
        builder.Configuration["EmailSender:Host"],
        builder.Configuration.GetValue<int>("EmailSender:Port"),
        builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
        builder.Configuration.GetValue<bool>("EmailSender:UseDefaultCredentials"),
        builder.Configuration["EmailSender:UserName"],
        builder.Configuration["EmailSender:Password"]
    )
);



// Agregar Razor Pages para que funcione con Identity
builder.Services.AddRazorPages();

// Agregar servicios de MVC (si estás usando controladores MVC también)
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // tiempo de expiración de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


app.UseSession(); // habilitar sesiones




// Configuración del middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); 

app.Run();

