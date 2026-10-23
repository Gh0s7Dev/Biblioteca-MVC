using BibliotecaMVC.Repositories;
using BibliotecaMVC.Services;
using BibliotecaMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
}).AddEntityFrameworkStores<BibliotecaContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddSingleton<IRepositorioLibro, RepositorioEnMemoria>();
builder.Services.AddScoped<IAutorService, AutorService>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Crear roles por defecto
    var defaultRoles = new[] { "Administrador", "Usuario" };

    foreach (var role in defaultRoles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(role));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear el rol {role}: " +
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
    }

    // Crear administrador por defecto
    const string adminEmail = "admin@admin.com";
    const string adminPassword = "Admin123!";

    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        admin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "No se pudo crear el administrador: " +
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // Garantizar que la cuenta tenga el rol Administrador
    if (!await userManager.IsInRoleAsync(admin, "Administrador"))
    {
        var roleResult = await userManager.AddToRoleAsync(
            admin, "Administrador");

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                "No se pudo asignar el rol Administrador: " +
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
