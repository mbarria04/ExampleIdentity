
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
using PracticaIdentity.Services;
using PriceRecalculationMvc_VS2022.Services;
using PriceRecalculationMvc_VS2022.Services.Background;
using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("IdentityConnection");

// =====================================
// DB CONTEXT + IDENTITY
// =====================================
builder.Services.AddDbContext<DB_ContextIdentity>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<DB_ContextIdentity>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// =====================================
// DB CONTEXT DE NEGOCIO
// =====================================
builder.Services.AddDbContextFactory<DB_Context>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
        sqlOptions.CommandTimeout(180);
    });
});

// =====================================
// AUTOFAC
// =====================================
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    FabricaServicios.RegisterServices(containerBuilder, builder.Configuration);
});

// =====================================
// COOKIES IDENTITY
// =====================================
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// =====================================
// EMAIL
// =====================================
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

// =====================================
// MVC + RAZOR + HTTP + SESSION
// =====================================
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient(); // ✅ para consumir la API interna

// Si usas sesiones, es buena práctica agregar cache distribuida (en memoria está ok para dev)
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// =====================================
// BACKGROUND / QUEUES
// =====================================
builder.Services.AddSingleton<IJobsQueue, JobsQueue>();
builder.Services.AddScoped<IJobsStore, JobsStore>();
builder.Services.AddScoped<IPriceCalculator, PriceCalculator>();
builder.Services.AddHostedService<PriceRecalculationConsumer>();
builder.Services.AddHostedService<NightlyScheduler>();

// =====================================
// ACTIVE DIRECTORY
// =====================================
builder.Services.AddScoped<ActiveDirectoryService>();

var app = builder.Build();

// =====================================
// MIDDLEWARE
// =====================================
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

// Orden recomendado: Auth → Session → Authorization → Endpoints
app.UseAuthentication();
app.UseSession();        // ✅ habilita sesiones antes de usar los controladores que leen HttpContext.Session
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
