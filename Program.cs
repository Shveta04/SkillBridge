
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkillBridge.Data;

var builder = WebApplication.CreateBuilder(args);

// Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddRazorPages();
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();

// Session-based protection for private pages
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "/";
    var normalizedPath = path.TrimEnd('/');

    // Public pages
    var publicPages = new[]
    {
        "/",
        "/Index",
        "/Account/Login",
        "/Account/Register",
        "/Admin/Login",
        "/Error"
    };

    if (publicPages.Contains(
        normalizedPath,
        StringComparer.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    // Admin pages require an Admin session
    if (normalizedPath.StartsWith(
        "/Admin",
        StringComparison.OrdinalIgnoreCase))
    {
        var adminEmail = context.Session.GetString("UserEmail");
        var adminRole = context.Session.GetString("UserRole");

        if (string.IsNullOrWhiteSpace(adminEmail) ||
            !string.Equals(
                adminRole,
                "Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect("/Admin/Login");
            return;
        }

        await next();
        return;
    }

    // Private user pages
    var privatePages = new[]
    {
        "/Dashboard",
        "/ResumeAnalyzer",
        "/SkillGap",
        "/CareerPath",
        "/Jobs",
        "/Courses",
        "/Profile",
        "/Account/Profile"
    };

    bool requiresLogin = privatePages.Contains(
        normalizedPath,
        StringComparer.OrdinalIgnoreCase);

    if (requiresLogin)
    {
        var userEmail = context.Session.GetString("UserEmail");

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            var returnUrl = context.Request.Path +
                            context.Request.QueryString;

            context.Response.Redirect(
                "/Account/Login?returnUrl=" +
                Uri.EscapeDataString(returnUrl));
            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();