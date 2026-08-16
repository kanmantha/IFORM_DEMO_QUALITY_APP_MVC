using IFormQualityApp.Data;
using IFormQualityApp.Models.Entities;
using IFormQualityApp.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Resolve the PostgreSQL connection string from Azure App Settings / environment
// (e.g. "ConnectionStrings__DefaultConnection" or "DATABASE_URL"), falling back to
// appsettings.json for local development.
var rawConnectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    Environment.GetEnvironmentVariable("DATABASE_URL") ??
    throw new InvalidOperationException(
        "No database connection string configured. Set the 'ConnectionStrings__DefaultConnection' " +
        "app setting (or DATABASE_URL) in your host before starting the app.");

var connectionString = NormalizePostgresConnectionString(rawConnectionString);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

// Trust the reverse proxy (Render terminates TLS). Required so
// UseHttpsRedirection and cookie security see the original scheme.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Seed database on startup. Fail fast with a clear message so hosting providers
// (Azure App Service) surface the real cause instead of hanging on a dead DB.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed. Verify the connection string and that the host can reach PostgreSQL.");
        throw;
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

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();

// Render (and many PaaS) expose Postgres as a URI like
// "postgres://user:pass@host:5432/dbname". Npgsql expects key-value
// connection strings, so translate the URI into that format here.
static string NormalizePostgresConnectionString(string value)
{
    var trimmed = value.Trim();

    if (!trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return trimmed;
    }

    var uri = new Uri(trimmed);
    var builder = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Database = uri.AbsolutePath.TrimStart('/')
    };

    if (!string.IsNullOrEmpty(uri.UserInfo))
    {
        var userInfo = uri.UserInfo.Split(':', 2);
        builder.Username = Uri.UnescapeDataString(userInfo[0]);
        if (userInfo.Length > 1)
        {
            builder.Password = Uri.UnescapeDataString(userInfo[1]);
        }
    }

    // Preserve any query-string options (e.g. ?sslmode=require)
    var query = uri.Query.TrimStart('?');
    if (!string.IsNullOrEmpty(query))
    {
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length != 2) continue;
            switch (kv[0].ToLowerInvariant())
            {
                case "sslmode":
                    builder.SslMode = Enum.Parse<Npgsql.SslMode>(kv[1], ignoreCase: true);
                    break;
                case "sslrootcert":
                    builder.RootCertificate = kv[1];
                    break;
            }
        }
    }

    return builder.ConnectionString;
}
